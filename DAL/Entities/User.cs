namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Пользователь
/// </summary>
public class User : BaseEntity
{
	/// <summary>
	/// электронная почта
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
	public ICollection<SpendingArea> SpendingAreas { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public ICollection<CashVault> CashVaults { get; set; }

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="User"/> 
	/// </summary>
	public User()
	{
		Transactions = new List<Transaction>();
		SpendingAreas = new List<SpendingArea>();
		CashVaults = new List<CashVault>();
	}
}
