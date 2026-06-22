namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Api модель ответа линейного графика трат по категориям
/// </summary>
public sealed class SpendingTrendLineApiResponse
{
    /// <summary>
    /// Упорядоченный список меток периодов
    /// </summary>
    public IReadOnlyCollection<string> Labels { get; set; } = [];

    /// <summary>
    /// Коллекция категорий с точками данных
    /// </summary>
    public IReadOnlyCollection<SpendingTrendLineCategoryApiModel> Categories { get; set; } = [];
}