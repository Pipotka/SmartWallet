using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Models.CashVault;

/// <summary>
/// Модель Api для создания денежного хранилища
/// </summary>
public class CreateTransactionEndpointApiModel
{
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