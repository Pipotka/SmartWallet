using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Services.Contracts.BackgroundService;

namespace Nasurino.SmartWallet.Services.BackgroundJobs
{
	/// <summary>
	/// Реализация <see cref="IDailyExpenseCategorieRecalculationService"/>.
	/// Считает сумму постингов по категории за день и атомарно записывает агрегат DailyExpenseCategorie.
	/// </summary>
	public sealed class DailyExpenseCategorieRecalculationService(
		IDailyExpenseCategorieRepository dailyExpenseCategorieRepository,
		IUnitOfWork unitOfWork) : IDailyExpenseCategorieRecalculationService
	{
		async Task IDailyExpenseCategorieRecalculationService.RecalculateAsync(
			Guid userId,
			Guid categoryId,
			DateTime day,
			CancellationToken cancellationToken)
		{
			var dayStart = day.Date;
			var dayEnd = day.Date.AddDays(1);

			var totalAmount = await unitOfWork.TransactionRepository.GetBalanceByAccountIdAndDateRangeAsync(
				categoryId,
				cancellationToken,
				new DateTimeOffset(dayStart),
				new DateTimeOffset(dayEnd));

			await dailyExpenseCategorieRepository.UpsertAsync(
				new DailyExpenseCategorie
				{
					UserId = userId,
					CategorieId = categoryId,
					Day = dayStart,
					TotalAmount = totalAmount
				},
				cancellationToken);
		}

		async Task IDailyExpenseCategorieRecalculationService.RecalculateManyAsync(
			Guid userId,
			IReadOnlyCollection<Guid> categoryIds,
			DateTime day,
			CancellationToken cancellationToken)
		{
			if (categoryIds is null || categoryIds.Count == 0)
			{
				return;
			}

			var dayStart = day.Date;
			var dayEnd = day.Date.AddDays(1);

			var balances = await unitOfWork.TransactionRepository.GetCategoryBalancesAsync(
				categoryIds,
				cancellationToken);

			var entities = categoryIds
				.Select(categoryId => new DailyExpenseCategorie
				{
					UserId = userId,
					CategorieId = categoryId,
					Day = dayStart,
					TotalAmount = balances.TryGetValue(categoryId, out var balance)
						? balance
						: 0m
				})
				.ToList();

			await dailyExpenseCategorieRepository.UpsertManyAsync(entities, cancellationToken);
		}
	}
}
