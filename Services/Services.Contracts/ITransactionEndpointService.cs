using Nasurino.SmartWallet.Services.Models.CreateModels;
using Nasurino.SmartWallet.Services.Models.DeleteModels;
using Nasurino.SmartWallet.Services.Models.Models;
using Nasurino.SmartWallet.Services.Models.UpdateModels;

namespace Nasurino.SmartWallet.Services.Contracts
{
	/// <summary>
	/// Интерфейс сервиса для работы с денежными хранилищами
	/// </summary>
	public interface ITransactionEndpointService
	{
		/// <summary>
		/// Создание нового денежного храшнилища
		/// </summary>
		Task<TransactionEndpointModel> CreateAsync(CreateTransactionEndpointModel model, CancellationToken token);

		/// <summary>
		/// Удаление денежного храшнилища
		/// </summary>
		Task DeleteAsync(DeleteTransactionEndpointModel model, CancellationToken token);

		/// <summary>
		/// Возвращет список денежных хранилищ по идентификатору пользователя
		/// </summary>
		Task<List<TransactionEndpointModel>> GetListByUserIdAsync(Guid userId, CancellationToken token);

		/// <summary>
		/// Обновление денежного храшнилища
		/// </summary>
		Task<TransactionEndpointModel> UpdateAsync(UpdateTransactionEndpointModel model, CancellationToken token);
	}
}