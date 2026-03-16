using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;
using Service.Infrastructure.Contracts;
using Services.Contracts;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис финансовой аналитики
/// </summary>
public sealed class FinancialAnalyticsService(IUnitOfWork unitOfWork, IFinancialCalculator calculator, IMapper mapper) : IFinancialAnalyticsService
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
}