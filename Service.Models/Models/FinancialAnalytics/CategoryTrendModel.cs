namespace Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

/// <summary>
/// Модель тренда по категории
/// </summary>
public sealed class CategoryTrendModel
{
    /// <summary>Идентификатор категории</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Название категории</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Процент изменения тренда</summary>
    public double TrendPercentage { get; set; }

    /// <summary>Сумма в текущем периоде</summary>
    public double CurrentPeriodAmount { get; set; }

    /// <summary> <inheritdoc /> </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not CategoryTrendModel trendModel)
        {
            return false;
        }
        return trendModel.CategoryId == CategoryId
            && trendModel.CategoryName == CategoryName;
    }

    /// <summary> <inheritdoc /> </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(CategoryId.GetHashCode(),
            CategoryName.GetHashCode());
    }
}