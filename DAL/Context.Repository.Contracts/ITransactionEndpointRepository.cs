using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts
{
	/// <summary>
	/// Репозиторий для работы с <see cref="TransactionEndpoint"/>
	/// </summary>
	public interface ITransactionEndpointRepository : IBaseWriteRepository<TransactionEndpoint>
	{
		/// <summary>
		/// Удаляет все конечные точки транзакций по идентификатору пользователя
		/// </summary>
		void DeleteTransactionEndpointsByUserId(Guid userId);

		/// <summary>
		/// Возвращает конечную точку транзакций пользователя по идентификатору
		/// </summary>
		Task<TransactionEndpoint?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает конечную точку транзакций по идентификатору
		/// </summary>
		Task<TransactionEndpoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает конечную точку транзакций по названию и идентификатору пользователя
		/// </summary>
		Task<TransactionEndpoint?> GetByNameAndUserIdAsync(Guid userId, string name, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает список конечных точек транзакций по идентификатору пользователя
		/// </summary>
		/// <remarks>Сначала идут конечные точки являющиеся хранилищами, а уже потом области трат</remarks>
		Task<List<TransactionEndpoint>> GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken);

		/// <summary>
		/// Очищает кэш значения категории
		/// </summary>
		Task ClearCategoryValueCacheAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает конечные точки транзакций пользователя по коллекции идентификаторов
		/// </summary>
		/// <param name="userId">Идентификатор пользователя</param>
		/// <param name="ids">Коллекция идентификаторов конечных точек</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task<List<TransactionEndpoint>> GetListByIdsAndUserIdAsync(
			Guid userId,
			IReadOnlyCollection<Guid> ids,
			CancellationToken cancellationToken);
	}
}