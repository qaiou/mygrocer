using Microsoft.AspNetCore.Mvc;
using MYGROCER.Data;
using Microsoft.Extensions.DependencyInjection;
using MYGROCER.Models;
using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MYGROCER.Patterns;
using MYGROCER.Services.Payments;

namespace MYGROCER.Controllers
{
    public class CartController : Controller
    {
        private const string CART_SESSION_KEY = "CartSession";
        private readonly AppDbContext _db;

        [ActivatorUtilitiesConstructor]
        public CartController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Cart
        // get cart for current context (DB for logged-in, session for guests)
        private async Task<CartModel> GetCartAsync()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId.HasValue)
            {
                // ensure a cart exists for this user
                var cart = await _db.Carts
                    .Include(c => c.CartItems)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId.Value);

                if (cart == null)
                {
                    cart = new CartModel
                    {
                        CustomerId = customerId.Value,
                        CartItems = new List<CartItemModel>(),
                        TotalPrice = 0m
                    };
                    _db.Carts.Add(cart);
                    await _db.SaveChangesAsync();
                }

                cart.TotalPrice = cart.CartItems.Sum(i => i.PricePerUnit * i.Quantity);
                return cart;
            }

            // guest: load from session
            var json = HttpContext.Session.GetString(CART_SESSION_KEY);
            if (string.IsNullOrEmpty(json))
                return new CartModel { CartItems = new List<CartItemModel>(), TotalPrice = 0m };

            try
            {
                var model = System.Text.Json.JsonSerializer.Deserialize<CartModel>(json);
                if (model?.CartItems == null) model = new CartModel { CartItems = new List<CartItemModel>() };
                model.TotalPrice = model.CartItems.Sum(i => i.PricePerUnit * i.Quantity);
                return model!;
            }
            catch
            {
                return new CartModel { CartItems = new List<CartItemModel>(), TotalPrice = 0m };
            }
        }

        // GET: /Cart/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = await GetCartAsync();
            return View(cart);
        }

        private CartModel GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(CART_SESSION_KEY);
            if (string.IsNullOrEmpty(json))
            {
                return new CartModel { CartItems = new List<CartItemModel>(), TotalPrice = 0m };
            }
            try
            {
                var model = JsonSerializer.Deserialize<CartModel>(json);
                if (model?.CartItems == null) model = new CartModel { CartItems = new List<CartItemModel>() };
                return model!;
            }
            catch
            {
                return new CartModel { CartItems = new List<CartItemModel>(), TotalPrice = 0m };
            }
        }

        private void SaveCartToSession(CartModel cart)
        {
            // ensure totals are calculated
            cart.TotalPrice = cart.CartItems?.Sum(i => i.PricePerUnit * i.Quantity) ?? 0m;
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CART_SESSION_KEY, json);
        }

        private async Task SaveCartAsync(CartModel cart)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId.HasValue)
            {
                // persist to DB
                var dbCart = await _db.Carts.Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId.Value);

                if (dbCart == null)
                {
                    dbCart = new CartModel { CustomerId = customerId.Value, CartItems = new List<CartItemModel>() };
                    _db.Carts.Add(dbCart);
                    await _db.SaveChangesAsync();
                }

                // load persisted items for this cart from the DB and remove them
                var existingItems = await _db.CartItems
                    .Where(ci => ci.CartId == dbCart.CartId)
                    .ToListAsync();
                _db.CartItems.RemoveRange(existingItems);
                await _db.SaveChangesAsync();

                // create a detached snapshot of incoming items to persist
                var itemsSnapshot = (cart.CartItems ?? new List<CartItemModel>())
                    .Select(i => new CartItemModel
                    {
                        CartId = dbCart.CartId,
                        ProductId = i.ProductId,
                        Name = i.Name,
                        PricePerUnit = i.PricePerUnit,
                        Quantity = i.Quantity
                    })
                    .ToList();

                // ensure any incoming CartItem instances are not tracked by the context
                foreach (var incoming in cart.CartItems ?? Enumerable.Empty<CartItemModel>())
                {
                    var entry = _db.Entry(incoming);
                    if (entry != null && entry.State != Microsoft.EntityFrameworkCore.EntityState.Detached)
                        entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                }

                // clear navigation so in-memory collection matches persisted state
                if (dbCart.CartItems != null)
                    dbCart.CartItems.Clear();

                // add the snapshot items in one batch
                _db.CartItems.AddRange(itemsSnapshot);

                dbCart.TotalPrice = cart.CartItems?.Sum(i => i.PricePerUnit * i.Quantity) ?? 0m;
                await _db.SaveChangesAsync();
                return;
            }

            // guest: save to session
            cart.TotalPrice = cart.CartItems?.Sum(i => i.PricePerUnit * i.Quantity) ?? 0m;
            var json = System.Text.Json.JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CART_SESSION_KEY, json);
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var cart = await GetCartAsync();
            // make sure prices are up to date from DB when possible
            foreach (var item in cart.CartItems!)
            {
                var product = await _db.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    item.PricePerUnit = product.BasePrice;
                    if (string.IsNullOrEmpty(item.Name)) item.Name = product.Name;
                }
            }
            cart.TotalPrice = cart.CartItems?.Sum(i => i.PricePerUnit * i.Quantity) ?? 0m;
            return View(cart);
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            // Require the user to be logged in (CustomerID stored in session by AccountController)
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            if (!customerId.HasValue)
            {
                if (isAjax)
                {
                    return Json(new { requireLogin = true });
                }
                TempData["Error"] = "Please log in before adding items to your cart.";
                return RedirectToAction("Login", "Account");
            }

            var product = await _db.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = await GetCartAsync();
            var existing = cart.CartItems!.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity += quantity;
                existing.PricePerUnit = product.BasePrice;
            }
            else
            {
                cart.CartItems.Add(new CartItemModel
                {
                    ProductId = productId,
                    Name = product.Name,
                    PricePerUnit = product.BasePrice,
                    Quantity = quantity
                });
            }

            await SaveCartAsync(cart);
            TempData["Success"] = $"Added '{product.Name}' to cart.";
            if (isAjax)
            {
                return Json(new { success = true });
            }
            return RedirectToAction("Index");
        }

        // GET: /Cart/Add (fallback link-based add to cart to avoid nested forms in views)
        [HttpGet]
        public async Task<IActionResult> Add(int productId, int quantity = 1, bool ajax = false)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (!customerId.HasValue)
            {
                TempData["Error"] = "Please log in before adding items to your cart.";
                return RedirectToAction("Login", "Account");
            }

            var product = await _db.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = await GetCartAsync();
            var existing = cart.CartItems!.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity += quantity;
                existing.PricePerUnit = product.BasePrice;
            }
            else
            {
                cart.CartItems.Add(new CartItemModel
                {
                    ProductId = productId,
                    Name = product.Name,
                    PricePerUnit = product.BasePrice,
                    Quantity = quantity
                });
            }

            await SaveCartAsync(cart);
            TempData["Success"] = $"Added '{product.Name}' to cart.";
            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            var cart = await GetCartAsync();
            var existing = cart.CartItems!.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null) cart.CartItems.Remove(existing);
            await SaveCartAsync(cart);
            TempData["Success"] = "Item removed from cart.";
            return RedirectToAction("Index");
        }

        // POST: /Cart/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm] string[]? items)
        {
            var cart = await GetCartAsync();
            if (items != null)
            {
                // items are in format productId:qty
                foreach (var entry in items)
                {
                    var parts = entry?.Split(':');
                    if (parts == null || parts.Length != 2) continue;
                    if (!int.TryParse(parts[0], out var pid)) continue;
                    if (!decimal.TryParse(parts[1], out var qty)) continue;

                    var existing = cart.CartItems!.FirstOrDefault(i => i.ProductId == pid);
                    if (existing != null)
                    {
                        if (qty <= 0) cart.CartItems.Remove(existing);
                        else existing.Quantity = qty;
                    }
                }
            }
            await SaveCartAsync(cart);
            TempData["Success"] = "Cart updated.";
            return RedirectToAction("Index");
        }

        // POST: /Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string paymentMethod, [FromForm] Dictionary<string, string?> details)
        {

            // Ensure user is logged in
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (!customerId.HasValue)
            {
                TempData["Error"] = "Please log in to complete checkout.";
                return RedirectToAction("Login", "Account");
            }


            // Gather cart and amount to charge
            var cart = await GetCartAsync();
            if (cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            // Load current product stock for validation
            var productIds = cart.CartItems.Select(i => i.ProductId).Distinct().ToList();
            var dbProducts = await _db.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId);

            foreach (var item in cart.CartItems)
            {
                if (!dbProducts.TryGetValue(item.ProductId, out var prod))
                {
                    TempData["Error"] = $"Product not found: {item.Name}";
                    return RedirectToAction("Index");
                }

                var qtyNeeded = Convert.ToInt32(item.Quantity);
                if (prod.StockQuantity < qtyNeeded)
                {
                    TempData["Error"] = $"Insufficient stock for '{prod.Name}'. Available: {prod.StockQuantity}, requested: {qtyNeeded}.";
                    return RedirectToAction("Index");
                }
            }

            var amount = cart.TotalPrice;

            // Use the payment factory from DI
            var factory = HttpContext.RequestServices.GetService(typeof(MYGROCER.Services.Payments.PaymentFactory)) as MYGROCER.Services.Payments.PaymentFactory;
            if (factory == null)
            {
                TempData["Error"] = "Payment service unavailable.";
                return RedirectToAction("Index");
            }

            // parse selected payment method into enum
            if (!System.Enum.TryParse<MYGROCER.Services.Payments.PaymentMethod>(paymentMethod, true, out var pm))
            {
                TempData["Error"] = "Invalid payment method.";
                return RedirectToAction("Index");
            }

            IPaymentProcessor processor;
            try
            {
                processor = factory.Create(pm);
            }
            catch
            {
                TempData["Error"] = "Invalid payment method.";
                return RedirectToAction("Index");
            }

            var detailsDict = Request.Form.ToDictionary(k => k.Key, v => (string?)v.Value.ToString());
            var result = await processor.ProcessPaymentAsync(amount, detailsDict ?? new Dictionary<string, string?>());
            if (!result.Success)
            {
                TempData["Error"] = "Payment failed: " + result.Message;
                return RedirectToAction("Index");
            }

            // Payment successful — update stock and persist order atomically
            using (var tx = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // Re-load products inside the transaction to avoid race conditions
                    foreach (var item in cart.CartItems)
                    {
                        var product = await _db.Products.FindAsync(item.ProductId);
                        if (product == null)
                        {
                            await tx.RollbackAsync();
                            TempData["Error"] = $"Product not found during finalize: {item.Name}";
                            return RedirectToAction("Index");
                        }

                        var qty = Convert.ToInt32(item.Quantity);
                        if (product.StockQuantity < qty)
                        {
                            await tx.RollbackAsync();
                            TempData["Error"] = $"Insufficient stock for '{product.Name}' during finalize. Please try again.";
                            return RedirectToAction("Index");
                        }

                        product.StockQuantity -= qty;
                        _db.Products.Update(product);
                    }

                    var order = new Order
                    {
                        CustomerId = customerId.Value,
                        OrderDate = DateTime.UtcNow,
                        PaymentMethod = paymentMethod,
                        TransactionId = result.TransactionId,
                        TotalAmount = amount,
                        Items = cart.CartItems.Select(i => new OrderItem
                        {
                            ProductId = i.ProductId,
                            ProductName = i.Name,
                            UnitPrice = i.PricePerUnit,
                            Quantity = i.Quantity,
                            ShippingStatus = "Pending"
                        }).ToList()
                    };

                    _db.Orders.Add(order);
                    await _db.SaveChangesAsync();
                    await tx.CommitAsync();

                    // --- OBSERVER PATTERN TRIGGER ---
                    // This executes immediately after the order is saved successfully
                    var notifier = new OrderNotifier();
                    notifier.Attach(new NotificationObserver());
                    notifier.Notify(order);
                    // --------------------------------
                }
                catch
                {
                    await tx.RollbackAsync();
                    // Note: if payment already charged, you should implement refund logic here.
                    TempData["Error"] = "An error occurred finalizing the order. Please contact support.";
                    return RedirectToAction("Index");
                }
            }

            // Clear cart on successful payment + stock update
            var empty = new CartModel { CartItems = new List<CartItemModel>(), TotalPrice = 0m };
            await SaveCartAsync(empty);
            TempData["Success"] = "Payment successful. Transaction " + result.TransactionId;

            return RedirectToAction("MyOrders");

        }

        // GET: /Cart/MyOrders
        [HttpGet]
        public IActionResult MyOrders()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (!customerId.HasValue)
            {
                TempData["Error"] = "Please log in to view your orders.";
                return RedirectToAction("Login", "Account");
            }

            var orders = _db.Orders
                .Where(o => o.CustomerId == customerId.Value)
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        [HttpGet]
        public IActionResult DbInfo()
        {
            var cs = _db.Database.GetDbConnection().ConnectionString;
            return Content(cs);
        }
    }
}