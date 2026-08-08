namespace Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;

/// <summary>
/// Модель категории с точками данных для линейного графика
/// </summary>
public sealed class SpendingTrendLineCategoryModel
{
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Название категории
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Коллекция точек данных по периодам
    /// </summary>
    public IReadOnlyCollection<SpendingTrendLineNodeModel> Nodes { get; set; } = [];
}