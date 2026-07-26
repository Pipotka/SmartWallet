namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Транзакция
/// </summary>
public sealed class Transaction : SmartDeletedEntity
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public User? User { get; set; }

	/// <summary>
	/// Тип транзакции
	/// </summary>
	public TransactionType Type { get; set; }

	/// <summary>
	/// Дата и время создания
	/// </summary>
	public DateTimeOffset MadeAt { get; set; }

	/// <summary>
	/// Записи движения средств по счетам, входящие в транзакцию.
	/// Сумма <see cref="Posting.Amount"/> всех постингов транзакции равна нулю.
	/// </summary>
	public ICollection<Posting> Postings { get; set; } = new List<Posting>();
}
