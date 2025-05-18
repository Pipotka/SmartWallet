namespace Nasurino.SmartWallet.Service.Models.Models;

/// <summary>
/// Модель транзакции
/// </summary>
public class TransactionModel
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
	public DateOnly MadeAt { get; set; }
}