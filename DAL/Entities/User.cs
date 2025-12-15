namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Пользователь
/// </summary>
public class User : BaseEntity
{
	/// <summary>
	/// Электронная почта
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	/// Имя
	/// </summary>
	public string FirstName { get; set; }
		
	/// <summary>
	/// Фамилия
	/// </summary>
	public string LastName { get; set; }

	/// <summary>
	/// Отчество
	/// </summary>
	public string Patronymic { get; set; }

	/// <summary>
	/// Хешированный пароль
	/// </summary>
	public string HashedPassword { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public ICollection<Transaction> Transactions { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public ICollection<TransactionEndpoint> CashVaults { get; set; }

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="User"/> 
	/// </summary>
	public User()
	{
		Transactions = new List<Transaction>();
		CashVaults = new List<TransactionEndpoint>();
	}
}
