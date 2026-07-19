using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.Context.Repository.Contracts.Specification;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="Transaction"/>
/// </summary>
public sealed class TransactionRepository : BaseWriteRepository<Transaction>, ITransactionRepository
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="TransactionRepository"/>
	/// </summary>
	public TransactionRepository(IDataStorageContext storage) : base(storage)
	{
	}

	async Task<PagedResult<TransactionData>> ITransactionRepository.GetPagedListByUserIdAsync(
		Guid userId,
		TransactionQuery query,
		CancellationToken cancellationToken)
	{
		var queryable = Storage.Read<Transaction>()
			.NotDeleted()
			.Where(x => x.UserId == userId);

		if (query.Type.HasValue)
		{
			queryable = queryable.Where(x => x.Type == query.Type.Value);
		}

		if (query.AccountId.HasValue)
		{
			var accountId = query.AccountId.Value;
			queryable = queryable
				.Where(x => x.Postings.Any(p => p.AccountId == accountId));
		}

		var totalCount = await queryable.CountAsync(cancellationToken);

		var items = await queryable
			.OrderByDescending(x => x.MadeAt)
			.Skip((query.Page - 1) * query.PageSize)
			.Take(query.PageSize)
			.Select(x => new TransactionData
			{
				Id = x.Id,
				UserId = x.UserId,
				Type = x.Type,
				MadeAt = x.MadeAt,
				Postings = x.Postings
					.Select(p => new PostingData
					{
						Id = p.Id,
						AccountId = p.AccountId,
						TransactionId = p.TransactionId,
						Amount = p.Amount
					})
					.ToList()
			})
			.ToListAsync(cancellationToken);

		return new PagedResult<TransactionData>
		{
			Items = items,
			TotalCount = totalCount,
			Page = query.Page,
			PageSize = query.PageSize,
			TotalPages = query.PageSize > 0
				? (int)Math.Ceiling((double)totalCount / query.PageSize)
				: 0
		};
	}

	Task<Transaction?> ITransactionRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
		=> Storage.Read<Transaction>().NotDeleted().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

	Task<Transaction?> ITransactionRepository.GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
		=> Storage.Read<Transaction>().NotDeleted()
			.Include(x => x.Postings)
			.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

	async Task<IReadOnlyCollection<Transaction>> ITransactionRepository.GetListByDateRangeAndUserIdAsync(
		Guid userId,
		DateTimeOffset startTimeRange,
		DateTimeOffset endTimeRange,
		CancellationToken cancellationToken)
		=> await Storage.Read<Transaction>().NotDeleted()
				.Where(InDateRange(startTimeRange, endTimeRange))
				.Where(x => x.UserId == userId)
				.ToListAsync(cancellationToken);

	/// <inheritdoc/>
	public override void Add(Transaction entity)
	{
		entity.MadeAt = DateTimeOffset.UtcNow;
		base.Add(entity);
	}

	void ITransactionRepository.DeleteTransactionsByTransactionEndpointIdAndDateRange(
		Guid transactionEndpointId,
		DateTimeOffset startDate = default,
		DateTimeOffset endDate = default)
	{
		endDate = endDate == default ? DateTimeOffset.MaxValue : endDate;
		DeleteEverythingBy(x => x.Postings.Any(p => p.AccountId == transactionEndpointId)
			&& startDate <= x.MadeAt && x.MadeAt < endDate);
	}

	void ITransactionRepository.DeleteTransactionsByUserId(Guid userId)
		=> DeleteEverythingBy(e => e.UserId == userId);

	async Task<IReadOnlyCollection<Transaction>> ITransactionRepository.GetListByDateRangeAndUserIdAsync(
		Guid userId,
		TransactionType transactionType,
		DateTimeOffset startDate,
		DateTimeOffset endDate,
		CancellationToken cancellationToken)
		=> await Storage.Read<Transaction>().NotDeleted()
				.Where(InDateRange(startDate, endDate))
				.Where(x => x.UserId == userId && x.Type == transactionType)
				.ToListAsync(cancellationToken);

	async Task<decimal> ITransactionRepository.GetBalanceByAccountIdAndDateRangeAsync(
		Guid accountId,
		CancellationToken cancellationToken,
		DateTimeOffset startDate = default,
		DateTimeOffset endDate = default)
	{
		endDate = endDate == default ? DateTimeOffset.MaxValue : endDate;

		return await Storage.Read<Posting>().NotDeleted()
			.Where(p => p.AccountId == accountId)
			.Where(p => p.Transaction!.MadeAt >= startDate && p.Transaction.MadeAt < endDate)
			.SumAsync(p => p.Amount, cancellationToken);
	}

	async Task<CategorizedSpendingResult> ITransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeAsync(
		Guid userId,
		DateTimeOffset startDate,
		DateTimeOffset endDate,
		CancellationToken cancellationToken)
	{
		var categories = await Storage.Read<Posting>().NotDeleted()
			.Where(p => p.Transaction!.MadeAt >= startDate && p.Transaction.MadeAt < endDate)
			.Where(p => p.Transaction!.UserId == userId
				&& p.Transaction.Type == TransactionType.Expense
				&& p.Account!.IsStorage == false)
			.GroupBy(p => new { p.AccountId, p.Account!.Name })
			.Select(g => new CategorySpendingItem
			{
				CategoryId = g.Key.AccountId,
				CategoryName = g.Key.Name,
				TotalAmount = g.Sum(p => p.Amount)
			})
			.OrderByDescending(cs => cs.TotalAmount)
			.ToListAsync(cancellationToken);

		return new CategorizedSpendingResult
		{
			TotalSpending = categories.Sum(c => c.TotalAmount),
			Categories = categories
		};
	}

	async Task<SpendingTrendLineResult> ITransactionRepository.GetSpendingTrendLineAsync(
		Guid userId,
		IReadOnlyCollection<DateRangeInfo> periods,
		CancellationToken cancellationToken)
	{
		var overallStart = periods.Min(p => p.Start);
		var overallEnd = periods.Max(p => p.End);
		var dbContext = (DbContext)Storage;

		var parameters = new List<object>();
		var paramIndex = 0;
		var periodSelects = new List<string>();

		foreach (var period in periods)
		{
			var labelParam = $"@p{paramIndex++}";
			var startParam = $"@p{paramIndex++}";
			var endParam = $"@p{paramIndex++}";

			periodSelects.Add($"SELECT {labelParam} AS label, {startParam} AS start_date, {endParam} AS end_date");
			parameters.Add(period.Label);
			parameters.Add(period.Start);
			parameters.Add(period.End);
		}

		var userIdParam = $"@p{paramIndex++}";
		var overallStartParam = $"@p{paramIndex++}";
		var overallEndParam = $"@p{paramIndex}";

		parameters.Add(userId);
		parameters.Add(overallStart);
		parameters.Add(overallEnd);

		var cte = string.Join(" UNION ALL ", periodSelects);

		var sql = $@"
			WITH periods AS ({cte})
			SELECT
				te.""Id"" AS ""CategoryId"",
				te.""Name"" AS ""CategoryName"",
				p.label AS ""Label"",
				SUM(po.""Amount"") AS ""TotalAmount""
			FROM ""Posting"" po
			INNER JOIN ""TransactionEndpoint"" te ON po.""AccountId"" = te.""Id""
				AND te.""DeletedAt"" IS NULL
			INNER JOIN ""Transaction"" t ON po.""TransactionId"" = t.""Id""
				AND t.""DeletedAt"" IS NULL
			INNER JOIN periods p ON t.""MadeAt"" >= p.start_date AND t.""MadeAt"" < p.end_date
			WHERE t.""DeletedAt"" IS NULL
				AND t.""UserId"" = {userIdParam}
				AND t.""Type"" = 1
				AND t.""MadeAt"" >= {overallStartParam}
				AND t.""MadeAt"" < {overallEndParam}
			GROUP BY te.""Id"", te.""Name"", p.label
			HAVING SUM(po.""Amount"") > 0";

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

	private static Expression<Func<Transaction, bool>> InDateRange(DateTimeOffset startTimeRange, DateTimeOffset endTimeRange)
		=> transaction => startTimeRange <= transaction.MadeAt && transaction.MadeAt < endTimeRange;
}
