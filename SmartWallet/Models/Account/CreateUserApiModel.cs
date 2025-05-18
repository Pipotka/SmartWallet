namespace Nasurino.SmartWallet.Models.Account;

/// <summary>
/// Api модель создания пользователя
/// </summary>
public class CreateUserApiModel
{
	/// <summary>
	/// электронная почта
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	/// Имя
	/// </summary>
	public string FirstName { get; set; }

	/// <summary>
	/// Фамилия
	/// </summary>
	public string LastName { get; set; }

	/// <summary>
	/// Отчество
	/// </summary>
	public string Patronymic { get; set; }

	/// <summary>
	/// Пароль
	/// </summary>
	public string Password { get; set; }
}