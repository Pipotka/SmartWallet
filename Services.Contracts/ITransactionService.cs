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
		/// Возвращет список транзакций по идентификатору пользователя
		/// </summary>
		Task<List<TransactionModel>> GetListByUserIdAsync(Guid userId, CancellationToken token);
	}
}