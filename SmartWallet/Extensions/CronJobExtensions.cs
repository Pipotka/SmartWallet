using Hangfire;
using Nasurino.SmartWallet.Services.Contracts.BackgroundService;

namespace Nasurino.SmartWallet.Extensions;

/// <summary>
/// Методы расширения для регистрации периодических задач.
/// </summary>
public static class CronJobExtensions
{
    /// <summary>
    /// Регистрирует периодические задачи приложения.
    /// </summary>
    public static void RegisterSmartWalletCronJobs(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var recurringJobManager = scope.ServiceProvider
            .GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate<IClearCategoryCacheService>(
            "clear-category-cache",
            service => service.ClearCategoryCacheAsync(),
            Cron.Monthly);
    }
}
