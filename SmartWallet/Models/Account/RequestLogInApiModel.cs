namespace Nasurino.SmartWallet.Models.Account;

/// <summary>
/// Api модель запроса пользователя на вход
/// </summary>
public class RequestLogInApiModel
{
	/// <summary>
	/// электронная почта
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	/// Пароль
	/// </summary>
	public string Password { get; set; }
}
