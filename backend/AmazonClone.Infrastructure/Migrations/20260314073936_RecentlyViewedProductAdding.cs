using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmazonClone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecentlyViewedProductAdding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecentlyViewedProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecentlyViewedProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecentlyViewedProducts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecentlyViewedProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyViewedProducts_ProductId",
                table: "RecentlyViewedProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyViewedProducts_UserId_ProductId",
                table: "RecentlyViewedProducts",
                columns: new[] { "UserId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyViewedProducts_UserId_ViewedAt",
                table: "RecentlyViewedProducts",
                columns: new[] { "UserId", "ViewedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecentlyViewedProducts");
        }
    }
}
