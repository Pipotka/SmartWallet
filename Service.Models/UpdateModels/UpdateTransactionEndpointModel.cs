namespace Nasurino.SmartWallet.Service.Models.UpdateModels;

/// <summary>
/// Модель обновления конечной точки транзакции
/// </summary>
public class UpdateTransactionEndpointModel
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; set; } = string.Empty;
	
	/// <summary>
	/// Ограничение трат
	/// </summary>
	public decimal? Limitation { get; set; }
}