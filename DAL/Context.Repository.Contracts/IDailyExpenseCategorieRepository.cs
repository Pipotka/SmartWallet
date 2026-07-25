using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts;

/// <summary>
/// Репозиторий для работы с <see cref="DailyExpenseCategorie"/>
/// </summary>
public interface IDailyExpenseCategorieRepository : IBaseWriteRepository<DailyExpenseCategorie>
{
	/// <summary>
	/// Возвращает категоризированные расходы пользователя за указанный период
	/// </summary>
	/// <param name="userId">Идентификатор пользователя</param>
	/// <param name="startDate">Начало периода (включительно)</param>
	/// <param name="endDate">Конец периода (исключительно)</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>
	/// Результат категоризации трат, содержащий общую сумму и коллекцию категорий с суммами расходов,
	/// отсортированную по убыванию суммы. Категории без трат за период не возвращаются.
	/// </returns>
	Task<CategorizedSpendingResult> GetCategorizedSpendingByUserIdAndDateRangeAsync(
		Guid userId,
		DateTimeOffset startDate,
		DateTimeOffset endDate,
		CancellationToken cancellationToken);

	/// <summary>
	/// Возвращает данные линейного графика трат по категориям за серию периодов
	/// </summary>
	/// <param name="userId">Идентификатор пользователя</param>
	/// <param name="periods">Коллекция временных диапазонов с метками</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>
	/// Результат, содержащий метки периодов и категории с суммами трат за каждый период.
	/// Категории без трат в конкретном периоде не содержат элемента для этого периода.
	/// </returns>
	Task<SpendingTrendLineResult> GetSpendingTrendLineAsync(
		Guid userId,
		IReadOnlyCollection<DateRangeInfo> periods,
		CancellationToken cancellationToken);
}
