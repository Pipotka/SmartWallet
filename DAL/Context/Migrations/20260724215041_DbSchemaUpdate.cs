using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nasurino.SmartWallet.Context.Migrations
{
    /// <inheritdoc />
    public partial class DbSchemaUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DailyExpenseCategorie_UserId",
                table: "DailyExpenseCategorie");

            migrationBuilder.CreateIndex(
                name: "IX_DailyExpenseCategorie_UserId_Day_Covering",
                table: "DailyExpenseCategorie",
                columns: new[] { "UserId", "Day", "CategorieId", "TotalAmount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DailyExpenseCategorie_UserId_Day_Covering",
                table: "DailyExpenseCategorie");

            migrationBuilder.CreateIndex(
                name: "IX_DailyExpenseCategorie_UserId",
                table: "DailyExpenseCategorie",
                column: "UserId");
        }
    }
}
