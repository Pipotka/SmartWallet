using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts
{
	/// <summary>
	/// Репозиторий для работы с <see cref="TransactionEndpoint"/>
	/// </summary>
	public interface ITransactionEndpointRepository : IBaseWriteRepository<TransactionEndpoint>
	{
		/// <summary>
		/// Удаляет все денежные хранилища по идентификатору пользователя
		/// </summary>
		void DeleteTransactionEndpointsByUserId(Guid userId);

		/// <summary>
		/// Возвращает денежное хранилище пользователя по идентификатору
		/// </summary>
		Task<TransactionEndpoint?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает денежное хранилище по идентификатору
		/// </summary>
		Task<TransactionEndpoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает денежное хранилище по названию и идентификатору пользователя
		/// </summary>
		Task<TransactionEndpoint?> GetByNameAndUserIdAsync(Guid userId, string name, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает список конечных точек по идентификатору пользователя
		/// </summary>
		/// <remarks>Сначала идут конечные точки являющиеся хранилищами, а уже потом области трат</remarks>
		Task<List<TransactionEndpoint>> GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken);
	}
}