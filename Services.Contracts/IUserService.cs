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
		/// Вход в аккаунт. Возвращает access-токен и refresh-токен
		/// </summary>
		Task<(string AccessToken, string RefreshToken)> LogInAsync(LogInModel model, CancellationToken token);

		/// <summary>
		/// Обновление access-токена по refresh-токену. Возвращает новый access-токен и новый refresh-токен
		/// </summary>
		Task<(string AccessToken, string RefreshToken)> RefreshAsync(string refreshToken, CancellationToken token);

		/// <summary>
		/// Выход из аккаунта — отзыв refresh-токена
		/// </summary>
		Task LogoutAsync(string refreshToken, CancellationToken token);

		/// <summary>
		/// Регистрация
		/// </summary>
		Task<UserModel> RegistrationAsync(CreateUserModel model, CancellationToken token);

		/// <summary>
		/// Обновление пользователя
		/// </summary>
		Task<UserModel> UpdateAsync(UpdateUserModel model, CancellationToken token);

		/// <summary>
		/// Смена пароля пользователя
		/// </summary>
		Task ChangePasswordAsync(ChangePasswordModel model, CancellationToken token);
	}
}
