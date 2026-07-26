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

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "TransactionEndpoint",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "Limitation",
                table: "TransactionEndpoint",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DailyExpenseCategorie_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Posting_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
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

            migrationBuilder.AlterColumn<double>(
                name: "Value",
                table: "TransactionEndpoint",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "Limitation",
                table: "TransactionEndpoint",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

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
