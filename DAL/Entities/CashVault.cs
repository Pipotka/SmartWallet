namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Денежное хранилище
/// </summary>
public class CashVault : SmartDeletedEntity
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
	/// Навигационное свойство
	/// </summary>
	public ICollection<Transaction> Transactions { get; set; }

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="CashVault"/> 
	/// </summary>
	public CashVault()
	{
		Transactions = new List<Transaction>();
	}
}
