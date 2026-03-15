namespace Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

/// <summary>
/// Расходы по категориям
/// </summary>
/// <param name="SpendingAmount">Общая сумма расходов</param>
/// <param name="CategorizedSpending">Категоризированные расходы</param>
public record SpendingCategoryModel(double SpendingAmount, IReadOnlyCollection<CategorySpendingItemModel> CategorizedSpending);