using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context;

namespace Nasurino.SmartWallet.Extensions;

/// <summary>
/// Методы расширения для регистрации инфраструктуры приложения в DI-контейнере.
/// </summary>
public static class InfrastructureExtensions
{
    /// <summary>
    /// Регистрирует инфраструктуру приложения
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDataBase(configuration);
        services.AddHangfireBackgroundJobs(configuration);

        return services;
    }

    /// <summary>
    /// Регистрирует базу данных.
    /// </summary>
    private static IServiceCollection AddDataBase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SmartWalletContext>(options => options
            .UseNpgsql(configuration.GetConnectionString("SmartWalletConnectionString")));

        return services;
    }

    /// <summary>
    /// Регистрирует Hangfire.
    /// </summary>
    private static IServiceCollection AddHangfireBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHangfire(conf => conf
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c =>
                c.UseNpgsqlConnection(configuration.GetConnectionString("HangfireConnection"))));
        services.AddHangfireServer();

        return services;
    }
}
