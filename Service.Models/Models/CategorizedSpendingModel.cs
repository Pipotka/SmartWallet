using System.Collections.Immutable;

namespace Nasurino.SmartWallet.Service.Models.Models;

/// <summary>
/// Расходы по категориям
/// </summary>
/// <param name="SpendingAmount">Общая сумма расходов</param>
/// <param name="CategorizedSpending">Категоризированные расходы</param>
public record CategorizedSpendingModel(double SpendingAmount, ImmutableDictionary<Guid, double> CategorizedSpending);