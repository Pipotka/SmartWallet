using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Entities.Contracts;
using System.Linq.Expressions;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Базовый репозиторий запись
/// </summary>
public abstract class BaseWriteRepository<TEntity> where TEntity : class 
{
	private readonly SmartWalletContext writer;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="BaseWriteRepository{TEntity}"/>
	/// </summary>
	protected BaseWriteRepository(SmartWalletContext writer)
	{
		this.writer = writer;
	}

	/// <summary>
	/// Добавить новую сущность
	/// </summary>
	public virtual void Add(TEntity entity)
		=> writer.Entry(entity).State = EntityState.Added;

	/// <summary>
	/// Изменить сущность
	/// </summary>
	public virtual void Update(TEntity entity)
		=> writer.Entry(entity).State = EntityState.Modified;

	/// <summary>
	/// Удалить сущность
	/// </summary>
	public virtual void Delete(TEntity entity)
	{
		if (entity is ISmartDeletedEntity smartDeletedEntity)
		{
			smartDeletedEntity.DeletedAt = DateTime.UtcNow;
			return;
		}
		writer.Entry(entity).State = EntityState.Deleted;
	}

	/// <summary>
	/// Удаляет все сущности по значению свойства
	/// </summary>
	protected virtual void DeleteEverythingBy(Expression<Func<TEntity, bool>> predicate)
	{
		var query = writer.Set<TEntity>().Where(predicate);
		if (typeof(ISmartDeletedEntity).IsAssignableFrom(typeof(TEntity)))
		{
			query.ExecuteUpdate(x => x
			.SetProperty(e => ((ISmartDeletedEntity)e).DeletedAt, DateTime.UtcNow));
			return;
		}
		query.ExecuteDelete();
	}
}
