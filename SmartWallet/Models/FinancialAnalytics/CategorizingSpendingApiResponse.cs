using System.Collections.Immutable;

namespace Nasurino.SmartWallet.Models.FinancialAnalytics;

/// <summary>
/// Модель Api ответа категоризации трат
/// </summary>
/// <param name="SpendingAmount">Сумма трат</param>
/// <param name="CategorizedSpending">Категоризированные траты</param>
public record CategorizingSpendingApiResponse(double SpendingAmount, ImmutableDictionary<Guid, double> CategorizedSpending);