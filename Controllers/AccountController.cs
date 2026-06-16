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