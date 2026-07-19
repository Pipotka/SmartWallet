namespace Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

/// <summary>
/// Модель узла линейного графика трат
/// </summary>
public sealed class SpendingTrendLineNodeModel
{
    /// <summary>
    /// Метка периода
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Сумма трат по категории за период
    /// </summary>
    public decimal Amount { get; set; }
}