using System;
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
            migrationBuilder.CreateTable(
                name: "DailyExpenseCategorie",
                columns: table => new
                {
                    CategorieId = table.Column<Guid>(type: "uuid", nullable: false),
                    Day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyExpenseCategorie", x => new { x.CategorieId, x.Day });
                    table.ForeignKey(
                        name: "FK_DailyExpenseCategorie_TransactionEndpoint_CategorieId",
                        column: x => x.CategorieId,
                        principalTable: "TransactionEndpoint",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DailyExpenseCategorie_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Posting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posting_TransactionEndpoint_AccountId",
                        column: x => x.AccountId,
                        principalTable: "TransactionEndpoint",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Posting_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyExpenseCategorie_UserId_Day_Covering",
                table: "DailyExpenseCategorie",
                columns: new[] { "UserId", "Day", "CategorieId", "TotalAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_Posting_AccountId",
                table: "Posting",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Posting_TransactionId",
                table: "Posting",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyExpenseCategorie");

            migrationBuilder.DropTable(
                name: "Posting");
        }
    }
}
