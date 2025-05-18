using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Entity.Configuration;

namespace Nasurino.SmartWallet.Context;

/// <summary>
/// Контекст работы с базой данных
/// </summary>
public class SmartWalletContext : DbContext, IDataStorageContext
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="SmartWalletContext"/>
	/// </summary>
	public SmartWalletContext(DbContextOptions<SmartWalletContext> options)
		: base(options)
	{
	}

	IQueryable<TEntity> IDataStorageContext.Read<TEntity>() where TEntity : class 
		=> Set<TEntity>().AsNoTracking().AsQueryable();

	void IDataStorageContext.Create<TEntity>(TEntity entity) where TEntity : class 
		=> Entry(entity).State = EntityState.Added;

	void IDataStorageContext.Delete<TEntity>(TEntity entity) where TEntity : class 
		=> Entry(entity).State = EntityState.Deleted;

	void IDataStorageContext.Update<TEntity>(TEntity entity)
		=> Entry(entity).State = EntityState.Modified;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartWalletAnchorEntity).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
