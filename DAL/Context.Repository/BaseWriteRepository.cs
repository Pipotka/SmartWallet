using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Entities.Contracts;

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
		=> writer.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Added;

	/// <summary>
	/// Изменить сущность
	/// </summary>
	public virtual void Update(TEntity entity)
		=> writer.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

	/// <summary>
	/// Удалить сущность
	/// </summary>
	public virtual void Delete(TEntity entity)
	{
		if (entity is SmartDeletedEntity smartDeletedEntity)
		{
			smartDeletedEntity.DeletedAt = DateTime.UtcNow;
			return;
		}
		writer.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
	}
}
