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
public class FinancialAnalyticsController : Controller
{
	private readonly IFinancialAnalyticsService financialAnalyticsService;
	private readonly IIdentityProvider identityProvider;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="FinancialAnalyticsController"/>
	/// </summary>
	public FinancialAnalyticsController(IFinancialAnalyticsService financialAnalyticsService,
		IIdentityProvider identityProvider)
	{
		this.financialAnalyticsService = financialAnalyticsService;
		this.identityProvider = identityProvider;
	}

	/// <summary>
	/// Получает категоризированные траты пользователя по месяцу года
	/// </summary>
	[HttpPut("categorized-spending")]
	[ProducesResponseType(typeof(CategorizingSpendingApiResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetCategorizingSpendingByMonthOfYear([FromBody] CategorizingSpendingApiRequest request, CancellationToken token)
	{
		var minDateInMonth = new DateTime(request.Year, request.Month, 1);
		var minDateInNextMonth = minDateInMonth.AddMonths(1);
		var result = await financialAnalyticsService.GetCategorizingSpendingByTimeRangeAndUserIdAsync(identityProvider.Id,
			minDateInMonth,
			minDateInNextMonth,
			request.AsPercentage,
			token);
		var response = new CategorizingSpendingApiResponse(result.SpendingAmount, result.CategorizedSpending);
		return Ok(response);
	}
}