namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Api модель категории линейного графика трат
/// </summary>
public sealed class SpendingTrendLineCategoryApiModel
{
    /// <summary>
    /// Название категории
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Коллекция точек данных по периодам
    /// </summary>
    public IReadOnlyCollection<SpendingTrendLineNodeApiModel> Nodes { get; set; } = [];
}