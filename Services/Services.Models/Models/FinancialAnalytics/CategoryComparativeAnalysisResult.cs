namespace Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;

/// <summary>
/// Результат сравнительного анализа трат по категориям
/// </summary>
public sealed class CategoryComparativeAnalysisResult
{
    /// <summary>Общая сумма трат во втором периоде</summary>
    public double TotalSecondPeriodSpending { get; set; }

    /// <summary>Общая сумма трат в первом периоде</summary>
    public double TotalFirstPeriodSpending { get; set; }

    /// <summary>Сравнительные анализы по категориям</summary>
    public IReadOnlyCollection<CategoryComparativeAnalysisModel> CategoryComparativeAnalyses { get; set; }
}