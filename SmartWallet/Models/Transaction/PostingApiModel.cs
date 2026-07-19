namespace Nasurino.SmartWallet.Models.Transaction;

/// <summary>
/// Api модель проводки (движение средств по счету)
/// </summary>
public sealed class PostingApiModel
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Идентификатор аккаунта
	/// </summary>
	public Guid AccountId { get; set; }

	/// <summary>
	/// Идентификатор транзакции
	/// </summary>
	public Guid TransactionId { get; set; }

	/// <summary>
	/// Сумма (со знаком: минус — списание, плюс — зачисление)
	/// </summary>
	public decimal Amount { get; set; }
}
