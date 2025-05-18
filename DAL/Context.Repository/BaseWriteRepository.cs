using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities.Contracts;
using System.Linq.Expressions;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Базовый репозиторий запись
/// </summary>
public abstract class BaseWriteRepository<TEntity> : IBaseWriteRepository<TEntity> where TEntity : class
{
	private readonly IDataStorageContext storage;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="BaseWriteRepository{TEntity}"/>
	/// </summary>
	protected BaseWriteRepository(IDataStorageContext storage)
	{
		this.storage = storage;
	}

	/// <summary>
	/// Добавить новую сущность
	/// </summary>
	public virtual void Add(TEntity entity)
		=> storage.Create(entity);

	/// <summary>
	/// Изменить сущность
	/// </summary>
	public virtual void Update(TEntity entity)
		=> storage.Update(entity);

	/// <summary>
	/// Удалить сущность
	/// </summary>
	public virtual void Delete(TEntity entity)
	{
		if (entity is ISmartDeletedEntity smartDeletedEntity)
		{
			smartDeletedEntity.DeletedAt = DateTime.UtcNow;
			storage.Update(smartDeletedEntity);
			return;
		}
		storage.Delete(entity);
	}

	/// <summary>
	/// Удаляет все сущности по значению свойства
	/// </summary>
	protected virtual void DeleteEverythingBy(Expression<Func<TEntity, bool>> predicate)
	{
		var query = storage.Read<TEntity>().Where(predicate);
		if (typeof(ISmartDeletedEntity).IsAssignableFrom(typeof(TEntity)))
		{
			query.ExecuteUpdate(x => x
			.SetProperty(e => ((ISmartDeletedEntity)e).DeletedAt, DateTime.UtcNow));
			return;
		}
		query.ExecuteDelete();
	}
}
