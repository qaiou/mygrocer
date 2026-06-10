using Microsoft.EntityFrameworkCore;
using MYGROCER.Models;

namespace MYGROCER.Data
{
    // ═══════════════════════════════════════════════════════════════
    // DATABASE LAYER — AppDbContext
    // This is the EF Core DbContext that manages all database tables.
    // It is registered as a service in Program.cs and injected via DI.
    // ═══════════════════════════════════════════════════════════════
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Each DbSet = one table in the database
        public DbSet<ProductsModel> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed some sample products so the app has data on first run
            modelBuilder.Entity<ProductsModel>().HasData(
                new ProductsModel { ProductId = 1, Name = "Fresh Milk 1L", Description = "Fresh full cream milk", Category = "Fresh Produce", BasePrice = 5.90m, StockQuantity = 50, ImageUrl = "/images/milk.jpg" },
                new ProductsModel { ProductId = 2, Name = "Basmati Rice 5kg", Description = "Premium long grain basmati rice", Category = "Packaged Goods", BasePrice = 22.50m, StockQuantity = 30, ImageUrl = "/images/rice.jpg" },
                new ProductsModel { ProductId = 3, Name = "Milo 1kg", Description = "Chocolate malt drink powder", Category = "Beverages", BasePrice = 18.90m, StockQuantity = 40, ImageUrl = "/images/milo.jpg" },
                new ProductsModel { ProductId = 4, Name = "Washing Detergent 2kg", Description = "Concentrated laundry powder", Category = "Household", BasePrice = 12.50m, StockQuantity = 25, ImageUrl = "/images/detergent.jpg" },
                new ProductsModel { ProductId = 5, Name = "Chicken Breast 500g", Description = "Fresh boneless chicken breast", Category = "Fresh Produce", BasePrice = 9.90m, StockQuantity = 20, ImageUrl = "/images/chicken.jpg" },
                new ProductsModel { ProductId = 6, Name = "Orange Juice 1L", Description = "100% pure squeezed orange juice", Category = "Beverages", BasePrice = 8.50m, StockQuantity = 35, ImageUrl = "/images/oj.jpg" }
            );
        }
    }
}
