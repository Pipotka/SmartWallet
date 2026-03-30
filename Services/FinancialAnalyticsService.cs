using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;
using Nasurino.SmartWallet.Services.Contracts;
using Service.Infrastructure.Contracts;
using Services.Contracts;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис финансовой аналитики
/// </summary>
public sealed class FinancialAnalyticsService(IUnitOfWork unitOfWork,
	IFinancialCalculator calculator,
	ISmartWalletValidateService validateService,
	IMapper mapper) : IFinancialAnalyticsService
{
	private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
	private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;

	async Task<SpendingCategoryModel> IFinancialAnalyticsService.GetCategorizingSpendingByDateRangeAndUserIdAsync(Guid userId,
		DateOnly startDate,
		DateOnly endDate,
		CancellationToken token)
	{
		_ = await _userRepository.GetUserByIdAsync(userId, token) 
			?? throw new EntityNotFoundByIdServiceException<User>(userId);

		var result = await _transactionRepository
			.GetCategorizedSpendingByUserIdAndDateRangeAsync(userId,
				startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
				endDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), token);

		return mapper.Map<SpendingCategoryModel>(result);
	}
	
	async Task<SpendingTrendAnalysisResult> IFinancialAnalyticsService.GetSpendingTrendAnalysisAsync(
		SpendingTrendAnalysisRequest request,
		CancellationToken token)
	{
		await validateService.ValidateAsync(request, token);
		
		var previousPeriodDateRange = request.GetFirstDateRange();
		var previousPeriod = await _transactionRepository
			.GetCategorizedSpendingByUserIdAndDateRangeAsync(request.UserId,
				previousPeriodDateRange.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
				previousPeriodDateRange.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
				token);
	
		var currentPeriodDateRange = request.GetSecondDateRange();
		var currentPeriod = await _transactionRepository
			.GetCategorizedSpendingByUserIdAndDateRangeAsync(request.UserId,
				currentPeriodDateRange.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
				currentPeriodDateRange.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
				token);
		var currentPeriodCategories = currentPeriod.Categories
			.ToDictionary(x => x.CategoryName); 

		var result = new SpendingTrendAnalysisResult
		{
			TotalCurrentSpending = currentPeriod.TotalSpending,
			TotalPreviousSpending = previousPeriod.TotalSpending,
			TotalSpendingTrendPercentage = calculator
				.CalculateTrendPercentage(currentPeriod.TotalSpending,
					previousPeriod.TotalSpending)
		};

		var categoryTrends = new LinkedList<CategoryTrendModel>();
		foreach (var previousCategory in previousPeriod.Categories) 
		{
			if (currentPeriodCategories.TryGetValue(previousCategory.CategoryName, out var currentCategory)) 
			{
				categoryTrends.AddFirst(new CategoryTrendModel
				{
					CategoryId = previousCategory.CategoryId,
					CategoryName = previousCategory.CategoryName,
					CurrentPeriodAmount = currentCategory.TotalAmount,
					TrendPercentage =calculator
						.CalculateTrendPercentage(currentCategory.TotalAmount,
							previousCategory.TotalAmount) 
				});	
			}
			else
			{
				categoryTrends.AddLast(new CategoryTrendModel
				{
					CategoryId = previousCategory.CategoryId,
					CategoryName = previousCategory.CategoryName,
					CurrentPeriodAmount = 0,
					TrendPercentage = -100
				}); 
			}
		}

		foreach (var newCategory in currentPeriod.Categories.Except(previousPeriod.Categories))
		{
			categoryTrends.AddLast(new CategoryTrendModel
			{
				CategoryId = newCategory.CategoryId,
				CategoryName = newCategory.CategoryName,
				CurrentPeriodAmount = newCategory.TotalAmount,
				TrendPercentage = 0
			});
		}

		result.CategoryTrends = categoryTrends;
		return result;
	}
}
