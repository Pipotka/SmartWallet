using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Агрегат дневных трат по области трат (конечной точке потока денег).
/// Составной ключ: идентификатор области трат + день.
/// </summary>
[Table(nameof(DailyExpenseCategorie))]
public class DailyExpenseCategorie
{
	/// <summary>
	/// Идентификатор области трат (конечной точки потока денег)
	/// </summary>
	[Key]
	[Column(Order = 0)]
	public Guid CategorieId { get; set; }

	/// <summary>
	/// Навигационное свойство — область трат
	/// </summary>
	public TransactionEndpoint? Category { get; set; }

	/// <summary>
	/// День (точность до дня)
	/// </summary>
	[Key]
	[Column(Order = 1)]
	public DateTime Day { get; set; }

	/// <summary>
	/// Суммарная сумма трат за день по области
	/// </summary>
	public double TotalAmount { get; set; } = 0.0;
}
