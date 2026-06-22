namespace Nasurino.SmartWallet.Context.Repository.Contracts.Models;

/// <summary>
/// Результат запроса данных линейного графика трат по категориям
/// </summary>
public sealed class SpendingTrendLineResult
{
    /// <summary>
    /// Метки периодов в хронологическом порядке
    /// </summary>
    public IReadOnlyCollection<string> Labels { get; set; } = [];

    /// <summary>
    /// Элементы трат по категориям за периоды
    /// </summary>
    public IReadOnlyCollection<SpendingTrendPeriodItem> PeriodItems { get; set; } = [];
}