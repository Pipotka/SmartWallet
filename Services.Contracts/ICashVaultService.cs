using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;

namespace Services.Contracts
{
	/// <summary>
	/// Интерфейс сервиса для работы с денежными хранилищами
	/// </summary>
	public interface ICashVaultService
	{
		/// <summary>
		/// Создание нового денежного храшнилища
		/// </summary>
		Task<CashVaultModel> CreateAsync(CreateCashVaultModel model, CancellationToken token);

		/// <summary>
		/// Удаление денежного храшнилища
		/// </summary>
		Task DeleteAsync(Guid userId, DeleteCashVaultModel model, CancellationToken token);

		/// <summary>
		/// Возвращет список денежных хранилищ по идентификатору пользователя
		/// </summary>
		Task<List<CashVaultModel>> GetListByUserIdAsync(Guid userId, CancellationToken token);

		/// <summary>
		/// Обновление денежного храшнилища
		/// </summary>
		Task<CashVaultModel> UpdateAsync(UpdateCashVaultModel model, CancellationToken token);
	}
}