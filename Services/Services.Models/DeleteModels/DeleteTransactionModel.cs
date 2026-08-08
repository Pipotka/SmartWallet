namespace Nasurino.SmartWallet.Services.Models.DeleteModels;

/// <summary>
/// Модель удаления транзакции
/// </summary>
public class DeleteTransactionModel
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }
	
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public Guid UserId { get; set; }
}