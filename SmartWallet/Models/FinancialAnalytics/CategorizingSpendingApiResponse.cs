namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Модель Api ответа категоризации трат
/// </summary>
/// <param name="SpendingAmount">Сумма трат</param>
/// <param name="CategorizedSpendingInPercent">Категоризированные траты в процентах</param>
public record CategorizingSpendingApiResponse(
    double SpendingAmount,
    Dictionary<Guid, double> CategorizedSpendingInPercent);