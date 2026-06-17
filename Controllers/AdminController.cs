using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYGROCER.Data;
using MYGROCER.Models;

namespace MYGROCER.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Orders()
        {
            var orders = _db.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View("~/Views/Orders/Index.cshtml", orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateShipping(int orderItemId, string status)
        {
            var item = _db.OrderItems.Find(orderItemId);
            if (item == null) return NotFound();

            item.ShippingStatus = status ?? "Pending";
            _db.SaveChanges();

            return RedirectToAction(nameof(Orders));
        }
    }
}