using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

namespace Services.Contracts
{
	/// <summary>
	/// Интерфейс сервиса финансовой аналитики
	/// </summary>
	public interface IFinancialAnalyticsService
	{
		/// <summary>
		/// Возвращает категоризированные траты пользователя в процентах по временному диапазону
		/// </summary>
		/// <param name="startDate">Начало временного диапазона</param>
		/// <param name="endDate">Конец временного диапазона</param>
		/// <param name="asPercentage">Флаг, указывающий, что результаты должны быть в процентах</param>
		/// <remarks><paramref name="endDate"/> - исключенный верхний предел временного диапазона</remarks>
		Task<SpendingCategoryModel> GetCategorizingSpendingByDateRangeAndUserIdAsync(Guid userId,
			DateOnly startDate,
			DateOnly endDate,
			bool asPercentage,
			CancellationToken token);
	}
}