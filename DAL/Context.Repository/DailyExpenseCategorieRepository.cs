using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.Context.Repository.Contracts.Specification;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="DailyExpenseCategorie"/>
/// </summary>
public sealed class DailyExpenseCategorieRepository : BaseWriteRepository<DailyExpenseCategorie>, IDailyExpenseCategorieRepository
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="DailyExpenseCategorieRepository"/>
	/// </summary>
	public DailyExpenseCategorieRepository(IDataStorageContext storage) : base(storage)
	{
	}

	async Task<CategorizedSpendingResult> IDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeAsync(
		Guid userId,
		DateTimeOffset startDate,
		DateTimeOffset endDate,
		CancellationToken cancellationToken)
	{
		var startDay = startDate.Date;
		var endDay = endDate.Date.AddDays(1);

		var categories = await Storage.Read<DailyExpenseCategorie>()
			.Where(x => x.UserId == userId
				&& x.Day >= startDay
				&& x.Day < endDay)
			.GroupBy(x => new { x.CategorieId, x.Category!.Name })
			.Select(g => new CategorySpendingItem
			{
				CategoryId = g.Key.CategorieId,
				CategoryName = g.Key.Name,
				TotalAmount = g.Sum(x => x.TotalAmount)
			})
			.OrderByDescending(cs => cs.TotalAmount)
			.ToListAsync(cancellationToken);

		return new CategorizedSpendingResult
		{
			TotalSpending = categories.Sum(c => c.TotalAmount),
			Categories = categories
		};
	}

	async Task<SpendingTrendLineResult> IDailyExpenseCategorieRepository.GetSpendingTrendLineAsync(
		Guid userId,
		IReadOnlyCollection<DateRangeInfo> periods,
		CancellationToken cancellationToken)
	{
		if (periods is null || periods.Count == 0)
		{
			return new SpendingTrendLineResult
			{
				Labels = new List<string>(),
				PeriodItems = new List<SpendingTrendPeriodItem>()
			};
		}

		var parameters = new List<object>();
		var paramIndex = 0;
		var valuesRows = new List<string>();

		foreach (var (period, index) in periods.Select((p, i) => (p, i)))
		{
			var ordParam = $"@p{paramIndex++}";
			var labelParam = $"@p{paramIndex++}";
			var startParam = $"@p{paramIndex++}";
			var endParam = $"@p{paramIndex++}";

			valuesRows.Add($"({ordParam}, {labelParam}, {startParam}::date, {endParam}::date)");
			parameters.Add(index + 1);
			parameters.Add(period.Label);
			parameters.Add(period.Start.Date);
			parameters.Add(period.End.Date.AddDays(1));
		}

		var userIdParam = $"@p{paramIndex++}";
		parameters.Add(userId);

		var valuesSql = string.Join(", ", valuesRows);

		var sql = $@"
			WITH periods(ord, label, start_date, end_date) AS (
				VALUES {valuesSql}
			),
			aggregated AS (
				SELECT
					dec.""CategorieId"" AS ""CategoryId"",
					p.label AS ""Label"",
					p.ord AS ""Ordinal"",
					SUM(dec.""TotalAmount"") AS ""TotalAmount""
				FROM ""DailyExpenseCategorie"" dec
				INNER JOIN periods p ON dec.""Day"" >= p.start_date AND dec.""Day"" < p.end_date
				WHERE dec.""UserId"" = {userIdParam}
				GROUP BY dec.""CategorieId"", p.label, p.ord
				HAVING SUM(dec.""TotalAmount"") > 0
			)
			SELECT
				a.""CategoryId"" AS ""CategoryId"",
				te.""Name"" AS ""CategoryName"",
				a.""Label"" AS ""Label"",
				a.""TotalAmount"" AS ""TotalAmount""
			FROM aggregated a
			INNER JOIN ""TransactionEndpoint"" te ON a.""CategoryId"" = te.""Id""
				AND te.""DeletedAt"" IS NULL
			ORDER BY a.""Ordinal"", te.""Name""";

		var dbContext = (DbContext)Storage;
		var periodItems = await dbContext.Database
			.SqlQueryRaw<SpendingTrendPeriodItem>(sql, parameters.ToArray())
			.AsNoTracking()
			.ToListAsync(cancellationToken);

		return new SpendingTrendLineResult
		{
			Labels = periods.Select(p => p.Label).ToList(),
			PeriodItems = periodItems
		};
	}
}
