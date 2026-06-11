using Nasurino.SmartWallet.Service.Models;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;

namespace Services.Contracts
{
	/// <summary>
	/// Интерфейс сервиса для работы с транзакциями
	/// </summary>
	public interface ITransactionService
	{
		/// <summary>
		/// Создание новой транзакции
		/// </summary>
		Task<TransactionModel> CreateAsync(CreateTransactionModel model, CancellationToken token);

		/// <summary>
		/// Удаление транзакции
		/// </summary>
		Task DeleteAsync(DeleteTransactionModel model, CancellationToken token);

		/// <summary>
		/// Возвращает постраничный список транзакций пользователя с фильтрацией
		/// </summary>
		Task<PagedResultModel<TransactionModel>> GetPagedListByUserIdAsync(Guid userId, TransactionQueryModel query, CancellationToken token);
	}
}