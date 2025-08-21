namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Модель Api запроса категоризации трат
/// </summary>
/// <param name="Month">Месяц</param>
/// <param name="Year">Год</param>
/// <param name="AsPercentage">Флаг, указывающий, что результаты должны быть в процентах</param>
public record CategorizingSpendingApiRequest(int Month, int Year, bool AsPercentage);