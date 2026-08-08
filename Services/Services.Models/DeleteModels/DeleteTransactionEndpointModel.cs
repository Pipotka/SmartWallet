namespace Nasurino.SmartWallet.Services.Models.DeleteModels;

/// <summary>
/// Модель удаления конечной точки транзакции
/// </summary>
public class DeleteTransactionEndpointModel
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