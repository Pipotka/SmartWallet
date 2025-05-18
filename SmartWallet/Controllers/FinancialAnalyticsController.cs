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
	/// Получает категоризированные траты пользователя в процентах по месяцу года
	/// </summary>
	[HttpPut("categorized-spending-in-percent")]
	[ProducesResponseType(typeof(CategorizingSpendingApiResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetCategorizingSpendingByMonthOfYear([FromBody] CategorizingSpendingApiRequest request, CancellationToken token)
	{
		var result = await financialAnalyticsService.GetCategorizingSpendingByMonthOfYearAndUserIdAsync(identityProvider.Id, request.MouthOfYear, token);
		var response = new CategorizingSpendingApiResponse
		{
			SpendingAmount = result.SpendingAmount,
			CategorizedSpendingInPercent = result.CategorizedSpendingInPercent
		};
		return Ok(response);
	}
}