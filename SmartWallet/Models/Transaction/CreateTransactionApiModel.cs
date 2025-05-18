namespace Nasurino.SmartWallet.Models.Transaction;

/// <summary>
/// Api модель создания транзакции
/// </summary>
public class CreateTransactionApiModel
{
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
}