using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;

namespace Services.Contracts
{
	/// <summary>
	/// Интерфейс сервиса для работы с пользователем
	/// </summary>
	public interface IUserService
	{
		/// <summary>
		/// Удаление пользователя
		/// </summary>
		Task DeleteAsync(DeleteUserModel model, CancellationToken token);

		/// <summary>
		/// Возвращает пользователя по Id
		/// </summary>
		Task<UserModel> GetUserByIdAsync(Guid userId, CancellationToken token);

		/// <summary>
		/// Вход в аккаунт
		/// </summary>
		Task<string> LogInAsync(LogInModel model, CancellationToken token);

		/// <summary>
		/// Регистрация
		/// </summary>
		Task<UserModel> RegistrationAsync(CreateUserModel model, CancellationToken token);

		/// <summary>
		/// Обновление пользователя
		/// </summary>
		Task<UserModel> UpdateAsync(UpdateUserModel model, CancellationToken token);
	}
}