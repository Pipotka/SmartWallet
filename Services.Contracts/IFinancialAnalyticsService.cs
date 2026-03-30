using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

namespace Nasurino.SmartWallet.Services.Contracts
{
	/// <summary>
	/// Интерфейс сервиса финансовой аналитики
	/// </summary>
	public interface IFinancialAnalyticsService
	{
		/// <summary>
		/// Возвращает категоризированные траты пользователя по временному диапазону
		/// </summary>
		/// <param name="startDate">Начало временного диапазона</param>
		/// <param name="endDate">Конец временного диапазона</param>
		/// <remarks><paramref name="endDate"/> - исключенный верхний предел временного диапазона</remarks>
		Task<SpendingCategoryModel> GetCategorizingSpendingByDateRangeAndUserIdAsync(Guid userId,
			DateOnly startDate,
			DateOnly endDate,
			CancellationToken token);

		/// <summary>
		/// Выполняет анализ трендов трат по категориям за два периода
		/// </summary>
		/// <param name="request">Параметры анализа трендов</param>
		/// <param name="token">Токен отмены</param>
		/// <returns>
		/// Результат анализа, содержащий общий тренд и тренды по каждой категории.
		/// Если категория присутствует только в предыдущем периоде, возвращается тренд -100% и сумма 0.
		/// Если категория присутствует только в текущем периоде, возвращается тренд 0% и сумма из текущего периода.
		/// </returns>
		Task<SpendingTrendAnalysisResult> GetSpendingTrendAnalysisAsync(
			SpendingTrendAnalysisRequest request,
			CancellationToken token);
	}
}
