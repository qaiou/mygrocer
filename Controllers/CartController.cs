using Microsoft.AspNetCore.Mvc;
using MYGROCER.Data;
using Microsoft.Extensions.DependencyInjection;
using MYGROCER.Models;
using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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

        // GET: /Cart/Checkout
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCartFromSession();
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

        // GET: /Cart
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            // make sure prices are up to date from DB when possible
            foreach (var item in cart.CartItems!)
            {
                var product = _db.Products.Find(item.ProductId);
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
            if (!customerId.HasValue)
            {
                TempData["Error"] = "Please log in before adding items to your cart.";
                return RedirectToAction("Login", "Account");
            }

            var product = await _db.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = GetCartFromSession();
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

            SaveCartToSession(cart);
            TempData["Success"] = $"Added '{product.Name}' to cart.";
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

            var cart = GetCartFromSession();
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

            SaveCartToSession(cart);
            TempData["Success"] = $"Added '{product.Name}' to cart.";
            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            var cart = GetCartFromSession();
            var existing = cart.CartItems!.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null) cart.CartItems.Remove(existing);
            SaveCartToSession(cart);
            TempData["Success"] = "Item removed from cart.";
            return RedirectToAction("Index");
        }

        // POST: /Cart/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update([FromForm] string[]? items)
        {
            var cart = GetCartFromSession();
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
            SaveCartToSession(cart);
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
            var cart = GetCartFromSession();
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

            var processor = factory.Create(paymentMethod);
            if (processor == null)
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
            SaveCartToSession(empty);
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
