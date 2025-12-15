namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Транзакция
/// </summary>
public class Transaction : SmartDeletedEntity
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
	/// Идентификатор аккаунта-источника 
	/// </summary>
	public Guid? SourceAccountId { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public TransactionEndpoint? SourceAccount { get; set; }

	/// <summary>
	/// Идентификатор аккаунта назначения
	/// </summary>
	public Guid? DestinationAccountId { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public TransactionEndpoint? DestinationAccount { get; set; }

	/// <summary>
	/// Значение
	/// </summary>
	public double Amount { get; set; } = 0.0;

	/// <summary>
	/// Дата и время создания
	/// </summary>
	public DateTime MadeAt { get; set; }
}
