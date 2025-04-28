namespace Nasurino.SmartWallet.Service.Models.UpdateModels;

/// <summary>
/// Модель обновления пользователя
/// </summary>
public class UpdateUserModel
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }

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
