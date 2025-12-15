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

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="FinancialAnalyticsController"/>
	/// </summary>
	public FinancialAnalyticsController(IFinancialAnalyticsService financialAnalyticsService,
		IIdentityProvider identityProvider)
	{
		_financialAnalyticsService = financialAnalyticsService;
		_identityProvider = identityProvider;
	}

	/// <summary>
	/// Получает категоризированные траты пользователя по месяцу года
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
			request.AsPercentage,
			token);
		var response = new CategorizingSpendingApiResponse(result.SpendingAmount, result.CategorizedSpending);
		return Ok(response);
	}
}