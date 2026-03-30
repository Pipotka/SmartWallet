namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Api ответ на анализ трендов трат
/// </summary>
public sealed class SpendingTrendAnalysisResponse
{
    /// <summary>
    /// Общая сумма трат в текущем периоде
    /// </summary>
    public double TotalCurrentSpending { get; set; }
    
    /// <summary>
    /// Общая сумма трат в предыдущем периоде
    /// </summary>
    public double TotalPreviousSpending { get; set; }
    
    /// <summary>
    /// Процент изменения общего тренда
    /// </summary>
    public double TotalSpendingTrendPercentage { get; set; }
    
    /// <summary>
    /// Тренды по категориям
    /// </summary>
    public IReadOnlyCollection<CategoryTrendApiModel> CategoryTrends { get; set; }
}