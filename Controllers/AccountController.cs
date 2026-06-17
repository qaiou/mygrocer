using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYGROCER.Data;
using MYGROCER.Models;
using System.Linq;

namespace MYGROCER.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserModel user)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "This email has already been taken.");
                    return View(user);
                }

                _context.Users.Add(user);
                _context.SaveChanges();
                
                return RedirectToAction("Login"); 
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            if (user != null)
            {
                HttpContext.Session.SetInt32("CustomerID", user.UserId);
                HttpContext.Session.SetString("CustomerName", user.FullName);

                // --- Merge session cart into DB cart (if present) ---
                try
                {
                    var json = HttpContext.Session.GetString("CartSession");
                    if (!string.IsNullOrEmpty(json))
                    {
                        var sessionCart = System.Text.Json.JsonSerializer.Deserialize<CartModel>(json);
                        if (sessionCart?.CartItems?.Any() == true)
                        {
                            var dbCart = _context.Carts
                                .Include(c => c.CartItems)
                                .FirstOrDefault(c => c.CustomerId == user.UserId);

                            if (dbCart == null)
                            {
                                dbCart = new CartModel
                                {
                                    CustomerId = user.UserId,
                                    CartItems = new List<CartItemModel>()
                                };
                                _context.Carts.Add(dbCart);
                                _context.SaveChanges();
                            }

                            foreach (var sItem in sessionCart.CartItems)
                            {
                                var existing = dbCart.CartItems.FirstOrDefault(ci => ci.ProductId == sItem.ProductId);
                                if (existing != null)
                                {
                                    existing.Quantity += sItem.Quantity;
                                }
                                else
                                {
                                    dbCart.CartItems.Add(new CartItemModel
                                    {
                                        CartId = dbCart.CartId,
                                        ProductId = sItem.ProductId,
                                        Name = sItem.Name,
                                        PricePerUnit = sItem.PricePerUnit,
                                        Quantity = sItem.Quantity
                                    });
                                }
                            }

                            dbCart.TotalPrice = dbCart.CartItems.Sum(ci => ci.PricePerUnit * ci.Quantity);
                            _context.SaveChanges();

                            // clear session cart after merging
                            HttpContext.Session.Remove("CartSession");
                        }
                    }
                }
                catch
                {
                    // swallow merge errors — do not prevent login
                }
                // --- end merge ---

                if (isAjax) return Json(new { success = true });
                return RedirectToAction("Index", "Home");
            }

            if (isAjax) return Json(new { success = false, message = "Invalid email or password." });
            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult MyOrders()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var orders = _context.Orders
                .Where(o => o.CustomerId == customerId.Value)
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

    }
}