namespace Nasurino.SmartWallet.Services.Contracts.BackgroundService;

/// <summary>
/// Фоновый сервис пересчёта ежедневных трат по категориям.
/// Используется для обновления агрегата <see cref="Nasurino.SmartWallet.Entities.DailyExpenseCategorie"/>
/// после изменения транзакций пользователя.
/// </summary>
public interface IDailyExpenseCategorieRecalculationService
{
	/// <summary>
	/// Пересчитывает сумму трат по категории за указанный день и атомарно записывает результат в БД.
	/// </summary>
	/// <param name="userId">Идентификатор пользователя</param>
	/// <param name="categoryId">Идентификатор категории трат (TransactionEndpoint)</param>
	/// <param name="day">Календарный день агрегации</param>
	/// <param name="cancellationToken">Токен отмены</param>
	Task RecalculateAsync(
		Guid userId,
		Guid categoryId,
		DateTime day,
		CancellationToken cancellationToken);

	/// <summary>
	/// Пересчитывает суммы трат по нескольким категориям за указанный день и атомарно
	/// записывает результаты в БД единым пакетом.
	/// </summary>
	/// <param name="userId">Идентификатор пользователя</param>
	/// <param name="categoryIds">Коллекция идентификаторов категорий трат</param>
	/// <param name="day">Календарный день агрегации</param>
	/// <param name="cancellationToken">Токен отмены</param>
	Task RecalculateManyAsync(
		Guid userId,
		IReadOnlyCollection<Guid> categoryIds,
		DateTime day,
		CancellationToken cancellationToken);
}
