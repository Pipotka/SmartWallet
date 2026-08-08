using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Nasurino.SmartWallet.Options;

namespace Nasurino.SmartWallet.Extensions;

/// <summary>
/// Методы расширения для регистрации классов конфигурации в DI-контейнере.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Регистрирует классы конфигурации приложения.
    /// </summary>
    public static IServiceCollection AddSmartWalletConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration
            .GetSection("ApiSettings:JwtSettings"));
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<JwtOptions>>().Value);

        services.Configure<BCryptOptions>(configuration
            .GetSection("ApiSettings:BCryptSettings"));

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto
                | ForwardedHeaders.XForwardedHost;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }
}
