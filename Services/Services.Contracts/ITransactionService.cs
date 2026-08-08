using Nasurino.SmartWallet.Services.Models;
using Nasurino.SmartWallet.Services.Models.CreateModels;
using Nasurino.SmartWallet.Services.Models.DeleteModels;
using Nasurino.SmartWallet.Services.Models.Models;

namespace Nasurino.SmartWallet.Services.Contracts
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