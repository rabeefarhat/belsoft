using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Belsoft.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PricingRule",
                table: "Orders",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Standard");

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Discount", "PricingRule" },
                values: new object[] { 0m, "Standard" });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Discount", "PricingRule" },
                values: new object[] { 0m, "Standard" });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Discount", "PricingRule" },
                values: new object[] { 0m, "Standard" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_Discount_NonNegative",
                table: "Orders",
                sql: "[Discount] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_Discount_NonNegative",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PricingRule",
                table: "Orders");
        }
    }
}
