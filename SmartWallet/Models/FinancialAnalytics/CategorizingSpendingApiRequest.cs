namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Модель Api запроса категоризации трат
/// </summary>
/// <param name="StartDate">Начальная дата диапазона</param>
/// <param name="EndDate">Конечная дата диапазона</param>
public record CategorizingSpendingApiRequest(DateOnly StartDate, DateOnly EndDate);