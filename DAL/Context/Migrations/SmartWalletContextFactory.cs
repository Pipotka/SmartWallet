using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nasurino.SmartWallet.Context;

namespace Nasurino.SmartWallet.Context.Migrations;

/// <summary>
/// Фабрика контекста для инструментов дизайн-времени (генерация миграций EF).
/// Позволяет создавать миграции без сборки стартап-проекта (SmartWallet),
/// который зависит от слоёв сервисов.
/// Строка подключения берётся из переменной окружения
/// ConnectionStrings__SmartWalletConnectionString (или SmartWalletConnectionString).
/// </summary>
public class SmartWalletContextFactory : IDesignTimeDbContextFactory<SmartWalletContext>
{
	/// <inheritdoc/>
	public SmartWalletContext CreateDbContext(string[] args)
	{
		var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SmartWalletConnectionString")
		                       ?? Environment.GetEnvironmentVariable("SmartWalletConnectionString")
		                       ?? "Host=localhost;Port=5432;Database=SmartWalletDb;Username=postgres;Password=postgres";

		var optionsBuilder = new DbContextOptionsBuilder<SmartWalletContext>()
			.UseNpgsql(connectionString);

		return new SmartWalletContext(optionsBuilder.Options);
	}
}
