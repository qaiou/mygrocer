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
        public DbSet<UserModel> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //product
           
             modelBuilder.Entity<ProductsModel>().HasData(
    new ProductsModel
    {
        ProductId = 1,
        Name = "Farm Fresh Full Cream Milk ",
        Description = "Farm Fresh Full Cream Milk 1L",
        Category = "Fresh Produce",
        BasePrice = 5.90m,
        StockQuantity = 50,
        ImageUrl = "https://www.farmfresh.com.my/wp-content/uploads/2021/10/FRESH-MILK-1L-UHT-SIDE-VIEW-768x768.png"
    },

    new ProductsModel
    {
        ProductId = 2,
        Name = "Maha Basmati Rice 5kg",
        Description = "Premium long grain basmati rice",
        Category = "Packaged Goods",
        BasePrice = 22.50m,
        StockQuantity = 30,
        ImageUrl = "https://myweekendplan.asia/wp-content/uploads/2022/07/Cap-Keluarga-Maha-Basmathi-Rice-.jpg"
    },

    new ProductsModel
    {
        ProductId = 3,
        Name = "Milo Chocolate Malt",
        Description = "Chocolate malt UHT Packet Drink",
        Category = "Food & Beverages",
        BasePrice = 18.90m,
        StockQuantity = 40,
        ImageUrl = "https://media.nedigital.sg/fairprice/fpol/media/images/product/XL/13004616_XL1_20230622.jpg"
    },

    new ProductsModel
    {
        ProductId = 4,
        Name = "Ariel Downy Laundry Detergent 4KG",
        Description = "Concentrated laundry powder",
        Category = "Home & Living",
        BasePrice = 15.50m,
        StockQuantity = 25,
        ImageUrl = "https://tse1.mm.bing.net/th/id/OIP.iJZJOSkW_gY6Q19tMQMEFQHaHa?rs=1&pid=ImgDetMain&o=7&rm=3"
    },

    new ProductsModel
    {
        ProductId = 5,
        Name = "Chicken Breast 1kg",
        Description = "Fresh boneless chicken breast",
        Category = "Fresh Produce",
        BasePrice = 10.21m,
        StockQuantity = 48,
        ImageUrl = "https://media.naheed.pk/catalog/product/cache/ff36c7bc52e2e5dbc63cd67fba513679/m/e/me1173797-4.jpg"
    },

    new ProductsModel
    {
        ProductId = 6,
        Name = "Sunquick Tropical 700ml",
        Description = "100% pure squeezed orange juice",
        Category = "Food & Beverages",
        BasePrice = 8.50m,
        StockQuantity = 35,
        ImageUrl = "https://aytacfood.co.uk/cdn/shop/products/sunquick-tropical-700ml-858288_1024x.jpg?v=1707828103"
    }
);
        }
    }
}
