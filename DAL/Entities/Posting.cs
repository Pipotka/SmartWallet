namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Запись движения средств по конкретному счёту в рамках транзакции.
/// Знак <see cref="Amount"/> кодирует направление: минус — расход со счёта, плюс — приход на счёт.
/// </summary>
public sealed class Posting : SmartDeletedEntity
{
	/// <summary>
	/// Идентификатор счёта (конечной точки потока денег)
	/// </summary>
	public Guid AccountId { get; set; }

	/// <summary>
	/// Навигационное свойство — счёт
	/// </summary>
	public TransactionEndpoint? Account { get; set; }

	/// <summary>
	/// Идентификатор транзакции
	/// </summary>
	public Guid TransactionId { get; set; }

	/// <summary>
	/// Навигационное свойство — транзакция
	/// </summary>
	public Transaction? Transaction { get; set; }

	/// <summary>
	/// Сумма движения средств со знаком (минус — расход, плюс — приход)
	/// </summary>
	public decimal Amount { get; set; }

	/// <summary>
	/// Дата создания проводки
	/// </summary>
	public DateTimeOffset CreatedAt { get; set; }
}
