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
            var amount = cart.TotalPrice;

            // Use the payment factory from DI
            // Collect form details into a dictionary (bank, cardNumber, expiry, cvv, etc.)
            var detailsDict = Request.Form.ToDictionary(k => k.Key, v => (string?)v.Value.ToString());

            var factory = HttpContext.RequestServices.GetService(typeof(MYGROCER.Services.Payments.PaymentFactory)) as MYGROCER.Services.Payments.PaymentFactory;
            if (factory == null)
            {
                TempData["Success"] = "Payment service unavailable.";
                return RedirectToAction("Index");
            }

            var processor = factory.Create(paymentMethod);
            if (processor == null)
            {
                TempData["Success"] = "Invalid payment method.";
                return RedirectToAction("Index");
            }

            var result = await processor.ProcessPaymentAsync(amount, detailsDict ?? new Dictionary<string, string?>());
            if (!result.Success)
            {
                TempData["Success"] = "Payment failed: " + result.Message;
                return RedirectToAction("Index");
            }

            // Persist order
            var order = new Order
            {
                CustomerId = HttpContext.Session.GetInt32("CustomerID") ?? 0,
                OrderDate = DateTime.UtcNow,
                PaymentMethod = paymentMethod,
                TransactionId = result.TransactionId,
                TotalAmount = amount,
                Items = cart.CartItems?.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.Name,
                    UnitPrice = i.PricePerUnit,
                    Quantity = i.Quantity
                }).ToList()
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Clear cart on successful payment
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
