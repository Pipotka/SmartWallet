namespace Nasurino.SmartWallet.Context.Repository.Contracts
{
	/// <summary>
	/// Базовый интерфейс записи репозитория
	/// </summary>
	public interface IBaseWriteRepository<TEntity> where TEntity : class
	{
		/// <summary>
		/// Добавить новую сущность
		/// </summary>
		void Add(TEntity entity);

		/// <summary>
		/// Удалить сущность
		/// </summary>
		void Delete(TEntity entity);

		/// <summary>
		/// Изменить сущность
		/// </summary>
		void Update(TEntity entity);
	}
}