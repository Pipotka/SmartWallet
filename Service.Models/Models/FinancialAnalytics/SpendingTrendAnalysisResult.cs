namespace Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

/// <summary>
/// Результат анализа трендов трат
/// </summary>
public sealed class SpendingTrendAnalysisResult
{
    /// <summary>Общая сумма трат в текущем периоде</summary>
    public double TotalCurrentSpending { get; set; }

    /// <summary>Общая сумма трат в прошлом периоде</summary>
    public double TotalPreviousSpending { get; set; }

    /// <summary>Тренд общей суммы трат</summary>
    public double TotalSpendingTrendPercentage { get; set; }

    /// <summary>Тренды по категориям</summary>
    public IReadOnlyCollection<CategoryTrendModel> CategoryTrends { get; set; }
}