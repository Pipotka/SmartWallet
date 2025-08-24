using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.Models;
using Service.Infrastructure.Contracts;
using Services.Contracts;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис финансовой аналитики
/// </summary>
public class FinancialAnalyticsService(IUnitOfWork unitOfWork, IFinancialCalculator calculator) : IFinancialAnalyticsService
{
	private readonly IUserRepository userRepository = unitOfWork.UserRepository;
	private readonly ITransactionRepository transactionRepository = unitOfWork.TransactionRepository;

	async Task<SpendingCategoryModel> IFinancialAnalyticsService.GetCategorizingSpendingByTimeRangeAndUserIdAsync(Guid userId,
		DateTime startTimeRange,
		DateTime endTimeRange,
		bool asPercentage,
		CancellationToken token)
	{
		if (await userRepository.GetUserByIdAsync(userId, token) is null) 
			throw new EntityNotFoundServiceException($"Пользователь с id = {userId} не найден.");

		var source = await transactionRepository.GetListByTimeRangeAndUserIdAsync(userId,
			NormalizeDateTime(startTimeRange),
			NormalizeDateTime(endTimeRange),
			token);

		var categorizedTransactions = source.GroupBy(x => x.ToSpendingAreaId).ToList();
		var spendingAmount = 0.0;
		var categorizedSpending = new Dictionary<Guid, double>();
		foreach (var category in categorizedTransactions)
		{
			categorizedSpending.Add(category.Key, 0.0);
			foreach (var transaction in category)
			{
				spendingAmount += transaction.Value;
				categorizedSpending[category.Key] += transaction.Value;
			}
		}

		if (asPercentage)
		{
			foreach (var category in categorizedSpending.Keys)
			{
				categorizedSpending[category] = calculator.GetPercentage(spendingAmount, categorizedSpending[category]);
			}
		}

		return new SpendingCategoryModel(spendingAmount, categorizedSpending);
	}

	static DateTime NormalizeDateTime(DateTime unnormalizedDateTime)
		=> unnormalizedDateTime.Kind switch
		{
			DateTimeKind.Utc => unnormalizedDateTime,
			DateTimeKind.Local => unnormalizedDateTime.ToUniversalTime(),
			_ => DateTime.SpecifyKind(unnormalizedDateTime, DateTimeKind.Utc),
		};
}