using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYGROCER.Data;
using MYGROCER.Models;

namespace MYGROCER.Controllers
{
    // ═══════════════════════════════════════════════════════════════════════════
    // BUSINESS LOGIC LAYER — ProductsController
    // ═══════════════════════════════════════════════════════════════════════════
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly DbConnectionSingleton _singleton;

        // Constructor Injection
        public ProductsController(AppDbContext db, DbConnectionSingleton singleton)
        {
            _db = db;
            _singleton = singleton;
        }

        // ─── PUBLIC: Product Listing Page (With Price + Category Filters) ───
        // GET: /Products
        public async Task<IActionResult> Index(string? category, string? search, decimal? minPrice, decimal? maxPrice)
        {
            // Singleton access logged
            var status = _singleton.GetStatus();
            ViewBag.SingletonStatus = status;

            // Start with all products
            var query = _db.Products.AsQueryable();

            // Filter by category if provided
            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category == category);

            // Filter by search term if provided
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name!.Contains(search) || p.Description!.Contains(search));

            // Filter by minimum price if provided
            if (minPrice.HasValue)
                query = query.Where(p => p.BasePrice >= minPrice.Value);

            // Filter by maximum price if provided
            if (maxPrice.HasValue)
                query = query.Where(p => p.BasePrice <= maxPrice.Value);

            // Get all distinct categories for the filter buttons/sidebar
            ViewBag.Categories = await _db.Products
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            // Store current filter values in ViewBag to keep UI state
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSearch = search;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            var products = await query.ToListAsync();
            return View(products);
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // ─── ADMIN: View All Products ────────────────────────────────────────
        // GET: /Products/AdminIndex
        public async Task<IActionResult> AdminIndex(string? search)
        {
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name!.Contains(search) || p.Category!.Contains(search));

            ViewBag.SingletonStatus = _singleton.GetStatus();
            ViewBag.CurrentSearch = search;

            var products = await query.ToListAsync();
            return View(products);
        }

        // ─── ADMIN: Add Product ──────────────────────────────────────────────
        // GET: /Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductsModel product)
        {
            if (ModelState.IsValid)
            {
                _db.Products.Add(product);
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Product '{product.Name}' added successfully!";
                return RedirectToAction(nameof(AdminIndex));
            }
            return View(product);
        }

        // ─── ADMIN: Edit Product ─────────────────────────────────────────────
        // GET: /Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductsModel product)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                _db.Update(product);
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Product '{product.Name}' updated successfully!";
                return RedirectToAction(nameof(AdminIndex));
            }
            return View(product);
        }

        // ─── ADMIN: Delete Product ───────────────────────────────────────────
        // GET: /Products/Delete/5 (confirmation page)
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Products/Delete/5 (actual deletion)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Product deleted successfully.";
            }
            return RedirectToAction(nameof(AdminIndex));
        }
    }
}