using Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;

namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Запрос на сравнительный анализ трат по категориям
/// </summary>
public class CategoryComparativeAnalysisApiRequest
{
    /// <summary>Дата окончания первого периода</summary>
    public DateOnly FirstPeriod { get; set; }

    /// <summary>Дата окончания второго периода</summary>
    public DateOnly SecondPeriod { get; set; }

    /// <summary>Единица измерения временного периода</summary>
    public TimeUnit TimeUnit { get; set; }

    /// <summary>Количество единиц в периоде</summary>
    public int TimeUnitCount { get; set; }
}