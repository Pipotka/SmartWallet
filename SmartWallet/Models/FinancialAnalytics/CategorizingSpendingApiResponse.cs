namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Модель Api ответа категоризации трат
/// </summary>
public class CategorizingSpendingApiResponse
{
	/// <summary>
	/// Сумма трат
	/// </summary>
	public double SpendingAmount {  get; set; }

	/// <summary>
	/// Категоризированные траты в процентах
	/// </summary>
	public Dictionary<Guid, double> CategorizedSpendingInPercent {  get; set; }
}