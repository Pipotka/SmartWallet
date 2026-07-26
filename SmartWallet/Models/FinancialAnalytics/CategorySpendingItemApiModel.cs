namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Api модель элемента категоризированных трат пользователя
/// </summary>
public sealed class CategorySpendingItemApiModel
{
    /// <summary>
    /// Идентификатор организации
    /// </summary>
    public Guid CategoryId { get; set; }
    
    /// <summary>
    /// Название категории
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;
    
    /// <summary>
    /// Сумма трат по категории
    /// </summary>
    public decimal TotalAmount { get; set; }
}