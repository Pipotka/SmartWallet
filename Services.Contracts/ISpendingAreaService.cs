using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;

namespace Services.Contracts
{
	/// <summary>
	/// Интерфейс сервиса для работы с областями трат
	/// </summary>
	public interface ISpendingAreaService
	{
		/// <summary>
		/// Создание новой области трат
		/// </summary>
		Task<SpendingAreaModel> CreateAsync(CreateSpendingAreaModel model, CancellationToken token);

		/// <summary>
		/// Удаление области трат
		/// </summary>
		Task DeleteAsync(Guid userId, DeleteSpendingAreaModel model, CancellationToken token);

		/// <summary>
		/// Возвращет список областей трат по идентификатору пользователя
		/// </summary>
		Task<List<SpendingAreaModel>> GetListByUserIdAsync(Guid userId, CancellationToken token);

		/// <summary>
		/// Обновление области трат
		/// </summary>
		Task<SpendingAreaModel> UpdateAsync(UpdateSpendingAreaModel model, CancellationToken token);
	}
}