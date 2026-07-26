namespace Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

/// <summary>
/// Модель сравнительного анализа по категории
/// </summary>
public sealed class CategoryComparativeAnalysisModel
{
    /// <summary>Идентификатор категории</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Название категории</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Сумма во втором периоде</summary>
    public decimal SecondPeriodAmount { get; set; }

    /// <summary>Сумма в первом периоде</summary>
    public decimal FirstPeriodAmount { get; set; }
}