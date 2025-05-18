using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Service.Exceptions;
using Services.Contracts;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис финансовой аналитики
/// </summary>
public class FinancialAnalyticsService(IUnitOfWork unitOfWork) : IFinancialAnalyticsService
{
	private readonly IUserRepository userRepository = unitOfWork.UserRepository;
	private readonly ITransactionRepository transactionRepository = unitOfWork.TransactionRepository;

	async Task<(double SpendingAmount, Dictionary<Guid, double> CategorizedSpendingInPercent)> IFinancialAnalyticsService.GetCategorizingSpendingByMonthOfYearAndUserIdAsync(Guid userId,
		DateOnly monthOfYear,
		CancellationToken token)
	{
		var user = await userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с id = {userId} не найден.");

		var source = await transactionRepository.GetListByMonthAndUserIdAsync(userId, monthOfYear, token);

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
		foreach (var category in categorizedSpending.Keys)
		{
			categorizedSpending[category] = GetPercentage(spendingAmount, categorizedSpending[category]);
		}

		return (spendingAmount, categorizedSpending);
	}

	public static double GetPercentage(double sum, double part, int decimals = 2)
		=> sum <= 0.0 ? 0.0 : Math.Round((part / sum) * 100, decimals);
}