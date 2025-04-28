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
	public User User { get; set; }

	/// <summary>
	/// Идентификатор денежного хранилища 
	/// </summary>
	public Guid FromCashVaultId { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public CashVault CashVault { get; set; }

	/// <summary>
	/// Идентификатор области трат
	/// </summary>
	public Guid ToSpendingAreaId { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public SpendingArea SpendingArea { get; set; }

	/// <summary>
	/// Значение
	/// </summary>
	public double Value { get; set; } = 0.0;

	/// <summary>
	/// Дата создания
	/// </summary>
	public DateTime MadeAt { get; set; }
}
