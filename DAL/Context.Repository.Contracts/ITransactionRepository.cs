using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts;

/// <summary>
/// Репозиторий для работы с <see cref="Transaction"/>
/// </summary>
public interface ITransactionRepository : IBaseWriteRepository<Transaction>
{
	/// <summary>
	/// Удаляет транзакций по идентификатору области трат, которые входят во временной диапазон
	/// </summary>
	/// <param name="startDate">Начало временного диапазона</param>
	/// <param name="endDate">Конец временного диапазона</param>
	void DeleteTransactionsByTransactionEndpointIdAndDateRange(Guid transactionEndpointId,
		DateTimeOffset startDate = default,
		DateTimeOffset endDate = default);

	/// <summary>
	/// Удаляет все транзакций по идентификатору пользователя
	/// </summary>
	void DeleteTransactionsByUserId(Guid userId);

	/// <summary>
	/// Возвращает транзакцию пользователя по идентификатору
	/// </summary>
	Task<Transaction?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);

	/// <summary>
	/// Возвращает транзакцию по идентификатору
	/// </summary>
	Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

	/// <summary>
	/// Возвращает список транзакций пользователя, созданный в указанный временной диапазон
	/// </summary>
	/// <param name="startDate">Начало временного диапазона</param>
	/// <param name="endDate">Конец временного диапазона</param>
	/// <remarks><paramref name="endDate"/> - исключенный верхний предел временного диапазона</remarks>
	Task<IReadOnlyCollection<Transaction>> GetListByDateRangeAndUserIdAsync(Guid userId,
		DateTimeOffset startDate,
		DateTimeOffset endDate,
		CancellationToken cancellationToken);

	/// <summary>
	/// Возвращает постраничный список транзакций пользователя с фильтрацией
	/// </summary>
	/// <param name="userId">Идентификатор пользователя</param>
	/// <param name="query">Параметры запроса с пагинацией и фильтрацией</param>
	/// <param name="cancellationToken">Токен отмены</param>
	Task<PagedResult<TransactionModel>> GetPagedListByUserIdAsync(Guid userId, TransactionQuery query, CancellationToken cancellationToken);

	/// <summary>
	/// Возвращает список транзакций пользователя по типу транзакции, созданных в указанный временной диапазон
	/// </summary>
	/// <param name="transactionType">Тип транзакции</param>
	/// <param name="startDate">Начало временного диапазона</param>
	/// <param name="endDate">Конец временного диапазона</param>
	/// <remarks><paramref name="endDate"/> - исключенный верхний предел временного диапазона</remarks>
	Task<IReadOnlyCollection<Transaction>> GetListByDateRangeAndUserIdAsync(Guid userId,
		TransactionType transactionType,
		DateTimeOffset startDate,
		DateTimeOffset endDate,
		CancellationToken cancellationToken);

	/// <summary>
	/// Получение баланса по идентификатору аккаунта
	/// </summary>
	/// <param name="accountId">Идентификатор аккаунта</param>
	/// <param name="startDate">Начало временного диапазона</param>
	/// <param name="endDate">Конец временного диапазона</param>
	Task<decimal> GetBalanceByAccountIdAndDateRangeAsync(Guid accountId, CancellationToken cancellationToken,
		DateTimeOffset startDate = default,
		DateTimeOffset endDate = default);
	
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