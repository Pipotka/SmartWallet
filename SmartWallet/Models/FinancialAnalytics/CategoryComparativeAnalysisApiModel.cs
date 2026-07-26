namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Api модель для передачи тренда по категории
/// </summary>
public sealed class CategoryComparativeAnalysisApiModel
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
    /// Сумма во втором периоде
    /// </summary>
    public decimal SecondPeriodAmount { get; set; }

    /// <summary>
    /// Сумма в первом периоде
    /// </summary>
    public decimal FirstPeriodAmount { get; set; }
}