using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

namespace Services.Contracts
{
	/// <summary>
	/// Интерфейс сервиса финансовой аналитики
	/// </summary>
	public interface IFinancialAnalyticsService
	{
		/// <summary>
		/// Возвращает категоризированные траты пользователя в процентах по месяцу года
		/// </summary>
		/// <param name="monthOfYear">месяц года</param>
		Task<SpendingCategoryModel> GetCategorizingSpendingByMonthOfYearAndUserIdAsync(Guid userId, DateOnly monthOfYear, CancellationToken token);
	}
}