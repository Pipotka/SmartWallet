using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Запрос на анализ трендов трат
/// </summary>
public class SpendingTrendAnalysisApiRequest
{
    /// <summary>Дата окончания первого (прошлого) периода</summary>
    public DateOnly FirstDate { get; set; }

    /// <summary>Дата окончания второго (текущего) периода</summary>
    public DateOnly SecondDate { get; set; }

    /// <summary>Единица измерения временного периода</summary>
    public TimeUnit TimeUnit { get; set; }

    /// <summary>Количество единиц в периоде</summary>
    public int TimeUnitCount { get; set; }
}