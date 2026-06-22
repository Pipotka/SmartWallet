namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Api модель узла линейного графика трат
/// </summary>
public sealed class SpendingTrendLineNodeApiModel
{
    /// <summary>
    /// Метка периода
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Сумма трат по категории за период
    /// </summary>
    public double Amount { get; set; }
}