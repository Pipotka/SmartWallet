using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Entity.Configuration;

namespace Nasurino.SmartWallet.Context;

/// <summary>
/// Контекст работы с базой данных
/// </summary>
public class SmartWalletContext : DbContext
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="SmartWalletContext"/>
	/// </summary>
	public SmartWalletContext(DbContextOptions<SmartWalletContext> options)
		: base(options)
	{
		Database.EnsureCreated();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartWalletAnchorEntity).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
