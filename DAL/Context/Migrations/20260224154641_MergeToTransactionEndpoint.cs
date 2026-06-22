using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nasurino.SmartWallet.Context.Migrations
{
    /// <inheritdoc />
    public partial class MergeToTransactionEndpoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_CashVault_FromCashVaultId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_SpendingArea_ToSpendingAreaId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "CashVault");

            migrationBuilder.DropTable(
                name: "SpendingArea");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_FromCashVaultId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_ToSpendingAreaId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "FromCashVaultId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ToSpendingAreaId",
                table: "Transaction");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Transaction",
                newName: "Amount");

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

            migrationBuilder.CreateTable(
                name: "TransactionEndpoint",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Limitation = table.Column<double>(type: "double precision", nullable: true),
                    IsStorage = table.Column<bool>(type: "boolean", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionEndpoint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionEndpoint_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_DestinationAccountId",
                table: "Transaction",
                column: "DestinationAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_SourceAccountId",
                table: "Transaction",
                column: "SourceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionEndpoint_UserId",
                table: "TransactionEndpoint",
                column: "UserId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_TransactionEndpoint_DestinationAccountId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_TransactionEndpoint_SourceAccountId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "TransactionEndpoint");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_DestinationAccountId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_SourceAccountId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "DestinationAccountId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "SourceAccountId",
                table: "Transaction");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Transaction",
                newName: "Value");

            migrationBuilder.AddColumn<Guid>(
                name: "FromCashVaultId",
                table: "Transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ToSpendingAreaId",
                table: "Transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CashVault",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashVault", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashVault_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SpendingArea",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpendingArea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpendingArea_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_FromCashVaultId",
                table: "Transaction",
                column: "FromCashVaultId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ToSpendingAreaId",
                table: "Transaction",
                column: "ToSpendingAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CashVault_UserId",
                table: "CashVault",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SpendingArea_UserId",
                table: "SpendingArea",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_CashVault_FromCashVaultId",
                table: "Transaction",
                column: "FromCashVaultId",
                principalTable: "CashVault",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_SpendingArea_ToSpendingAreaId",
                table: "Transaction",
                column: "ToSpendingAreaId",
                principalTable: "SpendingArea",
                principalColumn: "Id");
        }
    }
}
