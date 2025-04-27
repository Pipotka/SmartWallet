namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Область трат
/// </summary>
public class SpendingArea : SmartDeletedEntity
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public User User { get; set; }

	/// <summary>
	/// Значение
	/// </summary>
	public double Value { get; set; }

	/// <summary>
	/// Флаг возможности удаления пользователем
	/// </summary>
	public bool CanBeDeleted { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public ICollection<Transaction> Transactions { get; set; }

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="SpendingArea"/> 
	/// </summary>
	public SpendingArea()
	{
		Transactions = new List<Transaction>();
	}
}
