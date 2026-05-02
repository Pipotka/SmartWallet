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

	async Task<SpendingCategoryModel> IFinancialAnalyticsService.GetCategorizingSpendingAsync(CategorizingSpendingRequest request, CancellationToken token)
	{
        await validateService.ValidateAsync(request, token);

        _ = await _userRepository.GetUserByIdAsync(request.UserId, token) 
			?? throw new EntityNotFoundByIdServiceException<User>(request.UserId);

		var result = await _transactionRepository
			.GetCategorizedSpendingByUserIdAndDateRangeAsync(request.UserId,
                request.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                request.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), token);

		return mapper.Map<SpendingCategoryModel>(result);
	}
	
	async Task<SpendingTrendAnalysisResult> IFinancialAnalyticsService.GetSpendingTrendAnalysisAsync(
		SpendingTrendAnalysisRequest request,
		CancellationToken token)
	{
		await validateService.ValidateAsync(request, token);
		
		_ = await _userRepository.GetUserByIdAsync(request.UserId, token) 
		    ?? throw new EntityNotFoundByIdServiceException<User>(request.UserId);
		
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
			token.ThrowIfCancellationRequested();
			
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
			token.ThrowIfCancellationRequested();
			
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
