using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MYGROCER.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PaymentMethod = table.Column<string>(type: "TEXT", nullable: true),
                    TransactionId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                });

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

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    ShippingStatus = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "BasePrice", "Category", "Description", "ImageUrl", "Name", "StockQuantity" },
                values: new object[,]
                {
                    { 1, 5.90m, "Fresh Produce", "Farm Fresh Full Cream Milk 1L", "https://www.farmfresh.com.my/wp-content/uploads/2021/10/FRESH-MILK-1L-UHT-SIDE-VIEW-768x768.png", "Farm Fresh Full Cream Milk ", 50 },
                    { 2, 22.50m, "Packaged Goods", "Premium long grain basmati rice", "https://myweekendplan.asia/wp-content/uploads/2022/07/Cap-Keluarga-Maha-Basmathi-Rice-.jpg", "Maha Basmati Rice 5kg", 30 },
                    { 3, 18.90m, "Food & Beverages", "Chocolate malt UHT Packet Drink", "https://media.nedigital.sg/fairprice/fpol/media/images/product/XL/13004616_XL1_20230622.jpg", "Milo Chocolate Malt", 40 },
                    { 4, 15.50m, "Home & Living", "Concentrated laundry powder", "https://tse1.mm.bing.net/th/id/OIP.iJZJOSkW_gY6Q19tMQMEFQHaHa?rs=1&pid=ImgDetMain&o=7&rm=3", "Ariel Downy Laundry Detergent 4KG", 25 },
                    { 5, 10.21m, "Fresh Produce", "Fresh boneless chicken breast", "https://media.naheed.pk/catalog/product/cache/ff36c7bc52e2e5dbc63cd67fba513679/m/e/me1173797-4.jpg", "Chicken Breast 1kg", 48 },
                    { 6, 8.50m, "Food & Beverages", "100% pure squeezed orange juice", "https://aytacfood.co.uk/cdn/shop/products/sunquick-tropical-700ml-858288_1024x.jpg?v=1707828103", "Sunquick Tropical 700ml", 35 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "FullName", "Password", "Phone", "Role" },
                values: new object[] { 1, "demo@mygrocer.local", "Demo Customer", "password123", "0123456789", "Customer" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
