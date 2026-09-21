using Nasurino.SmartWallet.Entities;

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
	public decimal? Limitation { get; set; }
	
	/// <summary>
	/// Тип конечной точки
	/// </summary>
	public EndpointType EndpointType { get; set; }
}