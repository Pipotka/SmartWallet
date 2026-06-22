using Microsoft.EntityFrameworkCore;

namespace Nasurino.SmartWallet.Context
{
    /// <summary>
    /// Мигратор БД SmartWallet
    /// </summary>
    public static class SmartWalletMigrator
    {
        /// <summary>
        /// Мигрирует базу данных используя указанные <paramref name="options"/>
        /// </summary>
        /// <param name="options">опции контекста</param>
        public static async Task MigrateAsync(DbContextOptions<SmartWalletContext> options)
            => await new SmartWalletContext(options).Database.MigrateAsync();
    }
}
