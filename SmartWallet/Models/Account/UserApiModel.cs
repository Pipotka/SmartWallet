namespace Nasurino.SmartWallet.Models.Account;

/// <summary>
/// Модель Api пользователя
/// </summary>
public class UserApiModel
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }

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
}
