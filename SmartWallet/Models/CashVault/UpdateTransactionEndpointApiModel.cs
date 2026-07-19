namespace Nasurino.SmartWallet.Models.CashVault;

/// <summary>
/// Модель Api для обновления денежного хранилища
/// </summary>
public class UpdateTransactionEndpointApiModel
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
	/// Ограничение трат
	/// </summary>
	public decimal? Limitation { get; set; }
}