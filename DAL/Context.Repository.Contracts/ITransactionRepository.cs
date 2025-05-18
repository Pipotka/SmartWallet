using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts
{
	/// <summary>
	/// Репозиторий для работы с <see cref="Transaction"/>
	/// </summary>
	public interface ITransactionRepository : IBaseWriteRepository<Transaction>
	{
		/// <summary>
		/// Удаляет все транзакций по идентификатору области трат
		/// </summary>
		void DeleteTransactionsBySpendingAreaId(Guid spendingAreaId);

		/// <summary>
		/// Удаляет все транзакций по идентификатору пользователя
		/// </summary>
		void DeleteTransactionsByUserId(Guid userId);

		/// <summary>
		/// Возвращает транзакцию пользователя по идентификатору
		/// </summary>
		Task<Transaction?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает транзакцию по идентификатору
		/// </summary>
		Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает список транзакций по идентификатору пользователя
		/// </summary>
		Task<List<Transaction>> GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken);
	}
}