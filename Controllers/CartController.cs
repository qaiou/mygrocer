using Microsoft.AspNetCore.Mvc;
using MYGROCER.Data;
using Microsoft.Extensions.DependencyInjection;
using MYGROCER.Models;
using System.Text.Json;
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
        public IActionResult Checkout()
        {
            // for now, simply clear the cart and show success
            var cart = new CartModel { CartItems = new List<CartItemModel>(), TotalPrice = 0m };
            SaveCartToSession(cart);
            TempData["Success"] = "Checkout completed. Thank you for your order!";
            return RedirectToAction("Index", "Products");
        }
    }
}
