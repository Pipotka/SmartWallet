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
		var model = _mapper.Map<CategorizingSpendingRequest>(request);
		model.UserId = _identityProvider.Id;
        var result = await _financialAnalyticsService.GetCategorizingSpendingAsync(model, token);
		return Ok(_mapper.Map<CategorizingSpendingApiResponse>(result));
	}

	/// <summary>
	/// Получает сравнительный анализ по категориям за два периода
	/// </summary>
	[HttpPut("category-comparative-analysis")]
	[ProducesResponseType(typeof(CategoryComparativeAnalysisResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetCategoryComparativeAnalysis([FromBody] CategoryComparativeAnalysisApiRequest request, CancellationToken token)
	{
		var model = _mapper.Map<CategoryComparativeAnalysisRequest>(request);
		model.UserId = _identityProvider.Id;
		var result = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(model, token);
		return Ok(_mapper.Map<CategoryComparativeAnalysisResponse>(result));
	}
}
