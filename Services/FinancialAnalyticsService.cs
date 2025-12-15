using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;
using Service.Infrastructure.Contracts;
using Services.Contracts;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис финансовой аналитики
/// </summary>
public sealed class FinancialAnalyticsService(IUnitOfWork unitOfWork, IFinancialCalculator calculator) : IFinancialAnalyticsService
{
	private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
	private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;

	async Task<SpendingCategoryModel> IFinancialAnalyticsService.GetCategorizingSpendingByDateRangeAndUserIdAsync(Guid userId,
		DateOnly startDate,
		DateOnly endDate,
		bool asPercentage,
		CancellationToken token)
	{
		if (await _userRepository.GetUserByIdAsync(userId, token) is null) 
			throw new EntityNotFoundServiceException($"Пользователь с id = {userId} не найден.");

		var source = await _transactionRepository.GetListExpenseByDateRangeAndUserIdAsync(userId,
			startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
			endDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
			token);

		var categorizedTransactions = source.GroupBy(x => x.DestinationAccountId).ToList();
		var spendingAmount = 0.0;
		var categorizedSpending = new Dictionary<Guid, double>();
		foreach (var category in categorizedTransactions)
		{
			categorizedSpending.Add(category.Key!.Value, 0.0);
			foreach (var transaction in category)
			{
				spendingAmount += transaction.Amount;
				categorizedSpending[category.Key!.Value] += transaction.Amount;
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
}