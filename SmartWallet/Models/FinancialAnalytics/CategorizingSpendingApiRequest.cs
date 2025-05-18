namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Модель Api запроса категоризации трат
/// </summary>
public class CategorizingSpendingApiRequest
{
	/// <summary>
	/// Месяц года
	/// </summary>
	public DateOnly MouthOfYear { get; set; }
}