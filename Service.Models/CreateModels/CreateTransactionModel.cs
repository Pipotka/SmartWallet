namespace Nasurino.SmartWallet.Service.Models.CreateModels;

/// <summary>
/// Модель создания транзакции
/// </summary>
public class CreateTransactionModel
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