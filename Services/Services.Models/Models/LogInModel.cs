namespace Nasurino.SmartWallet.Services.Models.Models;

/// <summary>
/// Модель входа в систему
/// </summary>
public class LogInModel
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