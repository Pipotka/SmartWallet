namespace Nasurino.SmartWallet.Models.Account;

/// <summary>
/// Api модель обновления пользователя
/// </summary>
public class UpdateUserApiModel
{
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
}