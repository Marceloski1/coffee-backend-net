using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coffee.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryAndCleanCoffee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coffees_Categories_CategoryId",
                table: "Coffees");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Coffees_CategoryId",
                table: "Coffees");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Coffees");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Coffees",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Coffees",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Coffees");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Coffees");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Coffees",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Coffees_CategoryId",
                table: "Coffees",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Coffees_Categories_CategoryId",
                table: "Coffees",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
