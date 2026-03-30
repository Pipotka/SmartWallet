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
    
    /// <summary> <inheritdoc /> </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not CategorySpendingItem spendingModel)
        {
            return false;
        }
        return spendingModel.CategoryId == CategoryId
               && spendingModel.CategoryName == CategoryName;
    }

    /// <summary> <inheritdoc /> </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(CategoryId.GetHashCode(),
            CategoryName.GetHashCode());
    }
}