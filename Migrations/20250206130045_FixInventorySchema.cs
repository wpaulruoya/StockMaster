using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockMaster.Migrations
{
    /// <inheritdoc />
    public partial class FixInventorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateAdded",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "ItemName",
                table: "Inventories",
                newName: "Name");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Inventories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_UserId",
                table: "Inventories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_AspNetUsers_UserId",
                table: "Inventories",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_AspNetUsers_UserId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_UserId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Inventories",
                newName: "ItemName");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAdded",
                table: "Inventories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
