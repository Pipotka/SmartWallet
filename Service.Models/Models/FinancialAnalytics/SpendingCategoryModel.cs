namespace Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

/// <summary>
/// Расходы по категориям
/// </summary>
/// <param name="TotalSpending">Общая сумма расходов</param>
/// <param name="Categories">Категории с суммами трат</param>
public sealed record SpendingCategoryModel(decimal TotalSpending, IReadOnlyCollection<CategorySpendingItemModel> Categories);