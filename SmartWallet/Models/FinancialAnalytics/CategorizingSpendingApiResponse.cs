namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Модель Api ответа категоризации трат
/// </summary>
/// <param name="TotalSpending">Сумма трат</param>
/// <param name="Categories">Категоризированные траты</param>
public record CategorizingSpendingApiResponse(
    decimal TotalSpending,
    IReadOnlyCollection<CategorySpendingItemApiModel> Categories);