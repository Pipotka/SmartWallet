namespace Nasurino.SmartWallet.Context.Repository.Contracts.Models;

/// <summary>
/// Элемент трат по категории за период для линейного графика
/// </summary>
public sealed class SpendingTrendPeriodItem
{
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Название категории
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Метка периода
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Сумма трат по категории за период
    /// </summary>
    public decimal TotalAmount { get; set; }
}