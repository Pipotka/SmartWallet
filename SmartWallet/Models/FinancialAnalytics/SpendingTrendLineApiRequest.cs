using Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;

namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Модель Api запроса для линейного графика трат по категориям
/// </summary>
public sealed class SpendingTrendLineApiRequest
{
    /// <summary>
    /// Дата начала диапазона
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Дата окончания диапазона
    /// </summary>
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Единица измерения временного периода
    /// </summary>
    public TimeUnit TimeUnit { get; set; }
}