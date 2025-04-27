namespace Nasurino.SmartWallet.Service.Models.CreateModels;

/// <summary>
/// Модель создания пользователя
/// </summary>
public class CreateUserModel
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
