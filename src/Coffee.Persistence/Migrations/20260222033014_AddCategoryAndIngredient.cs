using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coffee.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Coffees",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

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
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CoffeeIngredients",
                columns: table => new
                {
                    CoffeesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IngredientsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeIngredients", x => new { x.CoffeesId, x.IngredientsId });
                    table.ForeignKey(
                        name: "FK_CoffeeIngredients_Coffees_CoffeesId",
                        column: x => x.CoffeesId,
                        principalTable: "Coffees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoffeeIngredients_Ingredients_IngredientsId",
                        column: x => x.IngredientsId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Coffees_CategoryId",
                table: "Coffees",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Coffees_CreatedAt",
                table: "Coffees",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Coffees_Name",
                table: "Coffees",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coffees_UpdatedAt",
                table: "Coffees",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatedAt",
                table: "Categories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UpdatedAt",
                table: "Categories",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CoffeeIngredients_IngredientsId",
                table: "CoffeeIngredients",
                column: "IngredientsId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_CreatedAt",
                table: "Ingredients",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_IsActive",
                table: "Ingredients",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_Name",
                table: "Ingredients",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_UpdatedAt",
                table: "Ingredients",
                column: "UpdatedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_Coffees_Categories_CategoryId",
                table: "Coffees",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coffees_Categories_CategoryId",
                table: "Coffees");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "CoffeeIngredients");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropIndex(
                name: "IX_Coffees_CategoryId",
                table: "Coffees");

            migrationBuilder.DropIndex(
                name: "IX_Coffees_CreatedAt",
                table: "Coffees");

            migrationBuilder.DropIndex(
                name: "IX_Coffees_Name",
                table: "Coffees");

            migrationBuilder.DropIndex(
                name: "IX_Coffees_UpdatedAt",
                table: "Coffees");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Coffees");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Coffees",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
