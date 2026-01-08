namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Конечная точка потока денег
/// </summary>
public class TransactionEndpoint : SmartDeletedEntity
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
	/// Название
	/// </summary>
	public string Name { get; set; } = string.Empty;
	
	/// <summary>
	/// Ограничение трат
	/// </summary>
	public double? Limitation { get; set; }
	
	/// <summary>
	/// Флаг указывающий, что конечная точка является денежным хранилищем
	/// </summary>
	public bool IsStorage { get; set; }
	
	/// <summary>
	/// Значение
	/// </summary>
	public double Value { get; set; } = 0.0;

	/// <summary>
	/// Транзакции, где текущая точка выступает как источник средств
	/// </summary>
	public ICollection<Transaction> OutgoingTransactions { get; set; } = new List<Transaction>();

	/// <summary>
	/// Транзакции, где текущая точка выступает как получатель средств
	/// </summary>
	public ICollection<Transaction> IncomingTransactions { get; set; } = new List<Transaction>();
}
