using Microsoft.AspNetCore.Mvc;
using MYGROCER.Data;
using MYGROCER.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

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
            if (user != null)
            {
                HttpContext.Session.SetInt32("CustomerID", user.UserId);
                HttpContext.Session.SetString("CustomerName", user.FullName);
                
                return RedirectToAction("Index", "Home");
            }
            
            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}