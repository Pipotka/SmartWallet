using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Models.FinancialAnalytics;
using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;
using Nasurino.SmartWallet.Services.Contracts;

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
		return Ok(_mapper.Map<CategorizingSpendingApiResponse>(result));
	}

	/// <summary>
	/// Выполняет анализ трендов трат по категориям за два периода
	/// </summary>
	[HttpPut("spending-trend-analysis")]
	[ProducesResponseType(typeof(SpendingTrendAnalysisResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetSpendingTrendAnalysis([FromBody] SpendingTrendAnalysisApiRequest request, CancellationToken token)
	{
		var model = _mapper.Map<SpendingTrendAnalysisRequest>(request);
		model.UserId = _identityProvider.Id;
		var result = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(model, token);
		return Ok(_mapper.Map<SpendingTrendAnalysisResponse>(result));
	}
}
