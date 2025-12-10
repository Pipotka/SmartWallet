using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nasurino.SmartWallet.Context.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Patronymic = table.Column<string>(type: "text", nullable: false),
                    HashedPassword = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashVault",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromCashVaultId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToSpendingAreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    MadeAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_CashVault_FromCashVaultId",
                        column: x => x.FromCashVaultId,
                        principalTable: "CashVault",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transaction_SpendingArea_ToSpendingAreaId",
                        column: x => x.ToSpendingAreaId,
                        principalTable: "SpendingArea",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transaction_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashVault_UserId",
                table: "CashVault",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SpendingArea_UserId",
                table: "SpendingArea",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_FromCashVaultId",
                table: "Transaction",
                column: "FromCashVaultId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ToSpendingAreaId",
                table: "Transaction",
                column: "ToSpendingAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_UserId",
                table: "Transaction",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "CashVault");

            migrationBuilder.DropTable(
                name: "SpendingArea");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
