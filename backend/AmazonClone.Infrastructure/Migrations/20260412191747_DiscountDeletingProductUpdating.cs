using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmazonClone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DiscountDeletingProductUpdating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Discounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiscountPersentage = table.Column<int>(type: "int", nullable: true),
                    DiscountPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Discounts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_IsActive_StartAt_EndAt",
                table: "Discounts",
                columns: new[] { "IsActive", "StartAt", "EndAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_ProductId",
                table: "Discounts",
                column: "ProductId");
        }
    }
}
