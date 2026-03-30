namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Api модель для передачи тренда по категории
/// </summary>
public sealed class CategoryTrendApiModel
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
    /// Процент изменения тренда
    /// </summary>
    public double TrendPercentage { get; set; }
    
    /// <summary>
    /// Сумма в текущем периоде
    /// </summary>
    public double CurrentPeriodAmount { get; set; }
}