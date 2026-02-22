using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmazonClone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserAsIdentityUserAdding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.RenameColumn(
                name: "IdentityUserId",
                table: "Orders",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "IdentityUserId",
                table: "Carts",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Orders",
                newName: "IdentityUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Carts",
                newName: "IdentityUserId");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentityUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }
    }
}
