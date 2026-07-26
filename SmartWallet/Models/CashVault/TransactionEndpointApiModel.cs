namespace Nasurino.SmartWallet.Models.CashVault;

/// <summary>
/// Модель Api денежного хранилища
/// </summary>
public class TransactionEndpointApiModel
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Значение
	/// </summary>
	public decimal Value { get; set; } = 0.0m;

	/// <summary>
	/// Ограничение трат
	/// </summary>
	public decimal? Limitation { get; set; }
	
	/// <summary>
	/// Флаг указывающий, что конечная точка является денежным хранилищем
	/// </summary>
	public bool IsStorage { get; set; }	
}