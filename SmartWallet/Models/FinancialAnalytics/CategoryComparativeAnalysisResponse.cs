namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Api ответ сравнительный анализ трат по категориям
/// </summary>
public sealed class CategoryComparativeAnalysisResponse
{
    /// <summary>
    /// Общая сумма трат во втором периоде
    /// </summary>
    public decimal TotalSecondPeriodSpending { get; set; }
    
    /// <summary>
    /// Общая сумма трат в первом периоде
    /// </summary>
    public decimal TotalFirstPeriodSpending { get; set; }

    /// <summary>
    /// Сравнительные анализы по категориям
    /// </summary>
    public IReadOnlyCollection<CategoryComparativeAnalysisApiModel> CategoryComparativeAnalyses { get; set; }
}