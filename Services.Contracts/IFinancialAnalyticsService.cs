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
        /// <param name="request">Запрос на получение категоризированных трат</param>
        Task<SpendingCategoryModel> GetCategorizingSpendingAsync(CategorizingSpendingRequest request, CancellationToken token);

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
		Task<CategoryComparativeAnalysisResult> GetCategoryComparativeAnalysisAsync(
			CategoryComparativeAnalysisRequest request,
			CancellationToken token);
	}
}
