namespace Nasurino.SmartWallet.Services.Contracts.BackgroundService
{
    /// <summary>
    /// Интерфейс фонового сервиса очистки кэша категорий
    /// </summary>
    public interface IClearCategoryCacheService
    {
        /// <summary>
        /// Очищает кэш категорий
        /// </summary>
        Task ClearCategoryCacheAsync();
    }
}
