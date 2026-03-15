using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Models.FinancialAnalytics;
using Services.Contracts;

namespace Nasurino.SmartWallet.Controllers;

/// <summary>
/// Контроллер для работы с аналитикой трат
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class FinancialAnalyticsController : Controller
{
	private readonly IFinancialAnalyticsService _financialAnalyticsService;
	private readonly IIdentityProvider _identityProvider;
	private readonly IMapper _mapper;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="FinancialAnalyticsController"/>
	/// </summary>
	public FinancialAnalyticsController(IFinancialAnalyticsService financialAnalyticsService,
		IIdentityProvider identityProvider,
		IMapper mapper)
	{
		_financialAnalyticsService = financialAnalyticsService;
		_identityProvider = identityProvider;
		_mapper = mapper;
	}

	/// <summary>
	/// Получает категоризированные траты пользователя по временному диапазону
	/// </summary>
	[HttpPut("categorized-spending")]
	[ProducesResponseType(typeof(CategorizingSpendingApiResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetCategorizingSpendingByDateRange([FromBody] CategorizingSpendingApiRequest request, CancellationToken token)
	{
		var result = await _financialAnalyticsService.GetCategorizingSpendingByDateRangeAndUserIdAsync(_identityProvider.Id,
			request.StartDate,
			request.EndDate,
			token);
		var response = new CategorizingSpendingApiResponse(result.SpendingAmount,
			_mapper.Map<IReadOnlyCollection<CategorySpendingItemApiModel>>(result.CategorizedSpending));
		return Ok(response);
	}
}