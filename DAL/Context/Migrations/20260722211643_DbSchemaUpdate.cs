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
            migrationBuilder.DropForeignKey(
                name: "FK_DailyExpenseCategorie_TransactionEndpoint_CategorieId",
                table: "DailyExpenseCategorie");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyExpenseCategorie_User_UserId",
                table: "DailyExpenseCategorie");

            migrationBuilder.DropForeignKey(
                name: "FK_Posting_Transaction_TransactionId",
                table: "Posting");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyExpenseCategorie_TransactionEndpoint_CategorieId",
                table: "DailyExpenseCategorie",
                column: "CategorieId",
                principalTable: "TransactionEndpoint",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyExpenseCategorie_User_UserId",
                table: "DailyExpenseCategorie",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posting_Transaction_TransactionId",
                table: "Posting",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyExpenseCategorie_TransactionEndpoint_CategorieId",
                table: "DailyExpenseCategorie");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyExpenseCategorie_User_UserId",
                table: "DailyExpenseCategorie");

            migrationBuilder.DropForeignKey(
                name: "FK_Posting_Transaction_TransactionId",
                table: "Posting");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyExpenseCategorie_TransactionEndpoint_CategorieId",
                table: "DailyExpenseCategorie",
                column: "CategorieId",
                principalTable: "TransactionEndpoint",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyExpenseCategorie_User_UserId",
                table: "DailyExpenseCategorie",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posting_Transaction_TransactionId",
                table: "Posting",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
