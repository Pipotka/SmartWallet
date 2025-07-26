using Nasurino.SmartWallet.Service.Models.Models;

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
		/// <param name="startTimeRange">Начало временного диапазона</param>
		/// <param name="endTimeRange">Конец временного диапазона</param>
		/// <param name="asPercentage">Флаг, указывающий, что результаты должны быть в процентах</param>
		/// <remarks><paramref name="endTimeRange"/> - исключенный верхний предел временного диапазона</remarks>
		Task<CategorizedSpendingModel> GetCategorizingSpendingByTimeRangeAndUserIdAsync(Guid userId,
			DateTime startTimeRange,
			DateTime endTimeRange,
			bool asPercentage,
			CancellationToken token);
	}
}