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
	
	async Task<CategoryComparativeAnalysisResult> IFinancialAnalyticsService.GetCategoryComparativeAnalysisAsync(
		CategoryComparativeAnalysisRequest request,
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

		var result = new CategoryComparativeAnalysisResult
		{
			TotalSecondPeriodSpending = currentPeriod.TotalSpending,
			TotalFirstPeriodSpending = previousPeriod.TotalSpending,
		};

		var categoryComparativeAnalyses = new LinkedList<CategoryComparativeAnalysisModel>();
		foreach (var previousCategory in previousPeriod.Categories) 
		{
			token.ThrowIfCancellationRequested();
			
			if (currentPeriodCategories.TryGetValue(previousCategory.CategoryName, out var currentCategory)) 
			{
				categoryComparativeAnalyses.AddFirst(new CategoryComparativeAnalysisModel
				{
					CategoryId = previousCategory.CategoryId,
					CategoryName = previousCategory.CategoryName,
					SecondPeriodAmount = currentCategory.TotalAmount,
					FirstPeriodAmount = previousCategory.TotalAmount
				});	
			}
			else
			{
				categoryComparativeAnalyses.AddLast(new CategoryComparativeAnalysisModel
				{
					CategoryId = previousCategory.CategoryId,
					CategoryName = previousCategory.CategoryName,
					SecondPeriodAmount = 0,
					FirstPeriodAmount = previousCategory.TotalAmount
				}); 
			}
		}

		foreach (var newCategory in currentPeriod.Categories.Except(previousPeriod.Categories))
		{
			token.ThrowIfCancellationRequested();

			categoryComparativeAnalyses.AddLast(new CategoryComparativeAnalysisModel
			{
				CategoryId = newCategory.CategoryId,
				CategoryName = newCategory.CategoryName,
				SecondPeriodAmount = newCategory.TotalAmount,
				FirstPeriodAmount = 0
			});
		}

        result.CategoryComparativeAnalyses = [.. categoryComparativeAnalyses.OrderByDescending(x => x.SecondPeriodAmount)];
		return result;
	}
}
