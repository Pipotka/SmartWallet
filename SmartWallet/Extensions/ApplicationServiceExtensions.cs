using Nasurino.SmartWallet.AutoMappers;
using Nasurino.SmartWallet.Common.Infrastructure;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using Nasurino.SmartWallet.Context;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Services;
using Nasurino.SmartWallet.Services.AutoMappers;
using Nasurino.SmartWallet.Services.BackgroundJobs;
using Nasurino.SmartWallet.Services.Contracts;
using Nasurino.SmartWallet.Services.Contracts.BackgroundService;
using Nasurino.SmartWallet.Services.Infrastructure;
using Nasurino.SmartWallet.Services.Infrastructure.Contracts;
using Nasurino.SmartWallet.Services.Validators;

namespace Nasurino.SmartWallet.Extensions;

/// <summary>
/// Методы расширения для регистрации сервисов приложения в DI-контейнере.
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Регистрирует сервисы приложения в DI-контейнере.
    /// </summary>
    public static IServiceCollection AddSmartWalletServices(this IServiceCollection services)
    {
        services.AddDataAccessLayer();
        services.AddRepositories();
        services.AddApplicationServices();
        services.AddAutoMappers();

        return services;
    }

    /// <summary>
    /// Регистрирует DbContext.
    /// </summary>
    private static IServiceCollection AddDataAccessLayer(this IServiceCollection services)
    {
        services.AddScoped<IDataStorageContext, SmartWalletContext>();

        return services;
    }

    /// <summary>
    /// Регистрирует репозитории и UnitOfWork.
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITransactionEndpointRepository, TransactionEndpointRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }

    /// <summary>
    /// Регистрирует прикладные сервисы.
    /// </summary>
    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IIdentityProvider, ApiIdentityProvider>();
        services.AddScoped<IFinancialCalculator, FinancialCalculator>();

        services.AddScoped<ITransactionEndpointService, TransactionEndpointService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IFinancialAnalyticsService, FinancialAnalyticsService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddScoped<ISmartWalletValidateService, SmartWalletValidateService>();
        services.AddScoped<IClearCategoryCacheService, ClearCategoryCacheService>();

        return services;
    }

    /// <summary>
    /// Регистрирует AutoMapper-профили.
    /// </summary>
    private static IServiceCollection AddAutoMappers(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ServiceModelMapper));
        services.AddAutoMapper(typeof(ApiModelMapper));

        return services;
    }
}
