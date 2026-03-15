namespace Nasurino.SmartWallet.Context.Repository.Contracts.Models;

/// <summary>
/// Элемент категоризированных трат пользователя
/// </summary>
public sealed class CategorySpendingItem
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
    public double TotalAmount { get; set; }
}