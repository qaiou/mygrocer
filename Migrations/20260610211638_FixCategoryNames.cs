using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MYGROCER.Migrations
{
    /// <inheritdoc />
    public partial class FixCategoryNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "BasePrice", "Category", "Description", "ImageUrl", "Name", "StockQuantity" },
                values: new object[,]
                {
                    { 1, 5.90m, "Fresh Produce", "Fresh full cream milk", "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=200", "Fresh Milk 1L", 50 },
                    { 2, 22.50m, "Packaged Goods", "Premium long grain basmati rice", "https://images.unsplash.com/photo-1586201375761-83865001e31c?w=200", "Basmati Rice 5kg", 30 },
                    { 3, 18.90m, "Food & Beverages", "Chocolate malt drink powder", "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6b/Milo_tin.jpg/200px-Milo_tin.jpg", "Milo 1kg", 40 },
                    { 4, 12.50m, "Home & Living", "Concentrated laundry powder", "https://images.unsplash.com/photo-1585421514284-efb74c2b69ba?w=200", "Washing Detergent 2kg", 25 },
                    { 5, 9.90m, "Fresh Produce", "Fresh boneless chicken breast", "https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=200", "Chicken Breast 500g", 20 },
                    { 6, 8.50m, "Food & Beverages", "100% pure squeezed orange juice", "https://images.unsplash.com/photo-1621506289937-a8e4df240d0b?w=200", "Orange Juice 1L", 35 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
