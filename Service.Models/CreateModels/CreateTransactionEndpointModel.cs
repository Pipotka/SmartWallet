namespace Nasurino.SmartWallet.Service.Models.CreateModels;

/// <summary>
/// Модель создания конечной точки транзакции
/// </summary>
public class CreateTransactionEndpointModel
{
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
	public double? Limitation { get; set; }
	
	/// <summary>
	/// Флаг указывающий, что конечная точка является денежным хранилищем
	/// </summary>
	public bool IsStorage { get; set; }
}