namespace Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;

/// <summary>
/// Расходы по категориям
/// </summary>
/// <param name="TotalSpending">Общая сумма расходов</param>
/// <param name="Categories">Категории с суммами трат</param>
public sealed record SpendingCategoryModel(double TotalSpending, IReadOnlyCollection<CategorySpendingItemModel> Categories);