namespace Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;

/// <summary>
/// Результат запроса данных линейного графика трат по категориям
/// </summary>
public sealed class SpendingTrendLineResult
{
    /// <summary>
    /// Упорядоченный список меток периодов
    /// </summary>
    public IReadOnlyCollection<string> Labels { get; set; } = [];

    /// <summary>
    /// Коллекция категорий с точками данных
    /// </summary>
    public IReadOnlyCollection<SpendingTrendLineCategoryModel> Categories { get; set; } = [];
}