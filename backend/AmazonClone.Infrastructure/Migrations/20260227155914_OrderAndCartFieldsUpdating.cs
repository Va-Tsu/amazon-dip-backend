using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmazonClone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrderAndCartFieldsUpdating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "Carts",
                newName: "CartKey");

            migrationBuilder.AddColumn<string>(
                name: "CartKey",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartKey",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "CartKey",
                table: "Carts",
                newName: "SessionId");
        }
    }
}
