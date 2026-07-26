using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nasurino.SmartWallet.Context.Migrations
{
    /// <inheritdoc />
    public partial class DropTransactionAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Колонка Amount (double precision, NOT NULL) — legacy из старой схемы,
            // не описана в EF-модели (сумма транзакции теперь считается через Posting).
            // EF не видит её в модели, поэтому удаляем явным SQL.
            migrationBuilder.Sql("ALTER TABLE \"Transaction\" DROP COLUMN \"Amount\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Transaction\" ADD COLUMN \"Amount\" double precision NOT NULL;");
        }
    }
}
