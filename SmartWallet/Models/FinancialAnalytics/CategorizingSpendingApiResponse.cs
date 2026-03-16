namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Модель Api ответа категоризации трат
/// </summary>
/// <param name="TotalSpending">Сумма трат</param>
/// <param name="Categories">Категоризированные траты</param>
public record CategorizingSpendingApiResponse(
    double TotalSpending,
    IReadOnlyCollection<CategorySpendingItemApiModel> Categories);