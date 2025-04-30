namespace Nasurino.SmartWallet.Service.Models.DeleteModels;

/// <summary>
/// Модель входа в систему
/// </summary>
public class DeleteUserModel
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Пароль
	/// </summary>
	public string Password { get; set; }
}