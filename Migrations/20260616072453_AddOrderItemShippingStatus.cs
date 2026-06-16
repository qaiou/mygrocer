using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MYGROCER.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemShippingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingStatus",
                table: "OrderItems",
                type: "TEXT",
                nullable: true,
                defaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingStatus",
                table: "OrderItems");
        }
    }
}
