using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYGROCER.Data;
using MYGROCER.Models;
using System.Diagnostics;

namespace MYGROCER.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        // Homepage — shows featured products from all categories
        public async Task<IActionResult> Index()
        {
            // Get up to 6 products to feature on homepage
            var featuredProducts = await _db.Products
                .OrderBy(p => p.Category)
                .Take(6)
                .ToListAsync();

            // Get category list for nav/filter
            ViewBag.Categories = await _db.Products
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            return View(featuredProducts);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
