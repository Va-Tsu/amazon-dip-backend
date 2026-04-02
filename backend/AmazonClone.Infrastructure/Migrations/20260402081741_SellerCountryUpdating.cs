using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmazonClone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SellerCountryUpdating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sellers_Countries_CountryId",
                table: "Sellers");

            migrationBuilder.DropIndex(
                name: "IX_Sellers_CountryId",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Sellers");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Sellers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Sellers");

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Sellers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_CountryId",
                table: "Sellers",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sellers_Countries_CountryId",
                table: "Sellers",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
