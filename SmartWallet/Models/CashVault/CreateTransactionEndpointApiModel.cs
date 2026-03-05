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
	public double? Limitation { get; set; }
	
	/// <summary>
	/// Флаг указывающий, что конечная точка является денежным хранилищем
	/// </summary>
	public bool IsStorage { get; set; }	
}