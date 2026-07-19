namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Агрегат дневных трат по области трат (конечной точке потока денег).
/// Составной ключ (CategorieId, Day) и маппинг таблицы задаются в DailyExpenseCategorieConfiguration.
/// </summary>
public class DailyExpenseCategorie
{
	/// <summary>
	/// Идентификатор области трат (конечной точки потока денег)
	/// </summary>
	public Guid CategorieId { get; set; }

	/// <summary>
	/// Навигационное свойство — область трат
	/// </summary>
	public TransactionEndpoint? Category { get; set; }

	/// <summary>
	/// День (точность до дня)
	/// </summary>
	public DateTime Day { get; set; }

	/// <summary>
	/// Суммарная сумма трат за день по области
	/// </summary>
	public decimal TotalAmount { get; set; }
}
