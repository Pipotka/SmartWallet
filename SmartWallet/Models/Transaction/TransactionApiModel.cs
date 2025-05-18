namespace Nasurino.SmartWallet.Models.Transaction;

/// <summary>
/// Api модель транзакции
/// </summary>
public class TransactionApiModel
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Идентификатор денежного хранилища 
	/// </summary>
	public Guid FromCashVaultId { get; set; }

	/// <summary>
	/// Идентификатор области трат
	/// </summary>
	public Guid ToSpendingAreaId { get; set; }

	/// <summary>
	/// Значение
	/// </summary>
	public double Value { get; set; } = 0.0;

	/// <summary>
	/// Дата создания
	/// </summary>
	public DateTime MadeAt { get; set; }
}