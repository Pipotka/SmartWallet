namespace Nasurino.SmartWallet.Context.Contracts;

/// <summary>
/// Интерфейс хранилища данных
/// </summary>
public interface IDataStorageContext
{
	/// <summary>
	/// Чтение данных из хранилища
	/// </summary>
	IQueryable<TEntity> Read<TEntity>() where TEntity : class;

	/// <summary>
	/// Добавление данных
	/// </summary>
	void Create<TEntity>(TEntity entity) where TEntity : class;

	/// <summary>
	/// Обновление данных
	/// </summary>
	void Update<TEntity>(TEntity entity) where TEntity : class;

	/// <summary>
	/// Удаление данных
	/// </summary>
	void Delete<TEntity>(TEntity entity) where TEntity : class;

	/// <summary>
	/// Асинхронное сохранение изменений
	/// </summary>
	Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
