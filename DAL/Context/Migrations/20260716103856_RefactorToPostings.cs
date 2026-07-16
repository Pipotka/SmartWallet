using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nasurino.SmartWallet.Context.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToPostings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_TransactionEndpoint_DestinationAccountId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_TransactionEndpoint_SourceAccountId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_DestinationAccountId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_SourceAccountId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "DestinationAccountId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "SourceAccountId",
                table: "Transaction");

            migrationBuilder.CreateTable(
                name: "DailyExpenseCategorie",
                columns: table => new
                {
                    CategorieId = table.Column<Guid>(type: "uuid", nullable: false),
                    Day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyExpenseCategorie", x => new { x.CategorieId, x.Day });
                    table.ForeignKey(
                        name: "FK_DailyExpenseCategorie_TransactionEndpoint_CategorieId",
                        column: x => x.CategorieId,
                        principalTable: "TransactionEndpoint",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.AddColumn<double>(
                name: "Amount",
                table: "Transaction",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationAccountId",
                table: "Transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceAccountId",
                table: "Transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_DestinationAccountId",
                table: "Transaction",
                column: "DestinationAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_SourceAccountId",
                table: "Transaction",
                column: "SourceAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_TransactionEndpoint_DestinationAccountId",
                table: "Transaction",
                column: "DestinationAccountId",
                principalTable: "TransactionEndpoint",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_TransactionEndpoint_SourceAccountId",
                table: "Transaction",
                column: "SourceAccountId",
                principalTable: "TransactionEndpoint",
                principalColumn: "Id");
        }
    }
}
