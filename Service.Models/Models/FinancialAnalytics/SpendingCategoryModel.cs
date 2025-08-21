namespace Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

/// <summary>
/// Траты по категориям
/// </summary>
/// <param name="SpendingAmount">Сумма трат</param>
/// <param name="CategorizedSpendingInPercent">Категоризированные траты в процентах</param>
public record SpendingCategoryModel(double SpendingAmount, Dictionary<Guid, double> CategorizedSpendingInPercent);