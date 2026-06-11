using Moq;
using Moq.Language.Flow;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.UnitTests.Services.Infrastructure.Mock.Extensions;

/// <summary>
/// Методы-расширения для моков <see cref="ITransactionRepository"/>
/// </summary>
public static class TransactionRepositoryMockExtensions
{
    /// <summary>
    /// Настраивает мок для возврата списка транзакций.
    /// <inheritdoc cref="ITransactionRepository.GetListByDateRangeAndUserIdAsync" path="/summary"/>
    /// </summary>
    /// <param name="mockedTransactionRepository">Мок репозитория транзакций</param>
    /// <param name="transactions">Список транзакций для возврата</param>
    /// <param name="userId">ID пользователя</param>
    /// <param name="startTimeRange">Начало временного диапазона</param>
    /// <param name="endTimeRange">Конец временного диапазона</param>
    /// <param name="token">Токен отмены</param>
    /// <remarks>
    /// <inheritdoc cref="ITransactionRepository.GetListByDateRangeAndUserIdAsync" path="/remarks"/>
    /// </remarks>
    public static void GetListByTimeRangeReturnValue(this Mock<ITransactionRepository> mockedTransactionRepository,
        IReadOnlyCollection<Transaction> transactions,
        Guid userId = default,
        DateTimeOffset startTimeRange = default,
        DateTimeOffset endTimeRange = default,
        CancellationToken token = default)
        => mockedTransactionRepository.SetupListByTimeRange(userId, startTimeRange, endTimeRange, token)
            .ReturnsAsync(transactions);
    
    /// <summary>
    /// Настраивает мок для возврата списка транзакций.
    /// <inheritdoc cref="ITransactionRepository.GetListByDateRangeAndUserIdAsync(System.Guid,Nasurino.SmartWallet.Entities.TransactionType,System.DateTimeOffset,System.DateTimeOffset,System.Threading.CancellationToken)" path="/summary"/>
    /// </summary>
    /// <param name="mockedTransactionRepository">Мок репозитория транзакций</param>
    /// <param name="transactions">Список транзакций для возврата</param>
    /// <param name="transactionType">Тип транзакции</param>
    /// <param name="userId">ID пользователя</param>
    /// <param name="startTimeRange">Начало временного диапазона</param>
    /// <param name="endTimeRange">Конец временного диапазона</param>
    /// <param name="token">Токен отмены</param>
    /// <remarks>
    /// <inheritdoc cref="ITransactionRepository.GetListByDateRangeAndUserIdAsync(System.Guid,Nasurino.SmartWallet.Entities.TransactionType,System.DateTimeOffset,System.DateTimeOffset,System.Threading.CancellationToken)" path="/remarks"/>
    /// </remarks>
    public static void GetTypedListByTimeRangeReturnValue(this Mock<ITransactionRepository> mockedTransactionRepository,
        IReadOnlyCollection<Transaction> transactions,
        Guid userId = default,
        TransactionType transactionType = TransactionType.ForTest,
        DateTimeOffset startTimeRange = default,
        DateTimeOffset endTimeRange = default,
        CancellationToken token = default)
        => mockedTransactionRepository.SetupListExpenseByTimeRange(userId, transactionType, startTimeRange, endTimeRange, token)
            .ReturnsAsync(transactions);

    private static ISetup<ITransactionRepository, Task<IReadOnlyCollection<Transaction>>> SetupListByTimeRange(
        this Mock<ITransactionRepository> mockedTransactionRepository,
        Guid userId = default,
        DateTimeOffset startTimeRange = default,
        DateTimeOffset endTimeRange = default,
        CancellationToken token = default)
    {
        return mockedTransactionRepository.Setup(x => x.GetListByDateRangeAndUserIdAsync(
            It.Is<Guid>(g => userId == Guid.Empty || g == userId),
            It.Is<DateTimeOffset>(d => startTimeRange == default || d == startTimeRange),
            It.Is<DateTimeOffset>(d => endTimeRange == default || d == endTimeRange),
            It.Is<CancellationToken>(ct => token == CancellationToken.None || ct == token)));
    }
    
    private static ISetup<ITransactionRepository, Task<IReadOnlyCollection<Transaction>>> SetupListExpenseByTimeRange(
        this Mock<ITransactionRepository> mockedTransactionRepository,
        Guid userId = default,
        TransactionType transactionType = TransactionType.ForTest,
        DateTimeOffset startTimeRange = default,
        DateTimeOffset endTimeRange = default,
        CancellationToken token = default)
    {
        return mockedTransactionRepository.Setup(x => x.GetListByDateRangeAndUserIdAsync(
            It.Is<Guid>(g => userId == Guid.Empty || g == userId),
            It.Is<TransactionType>(t => t == TransactionType.ForTest || t == transactionType),
            It.Is<DateTimeOffset>(d => startTimeRange == default || d == startTimeRange),
            It.Is<DateTimeOffset>(d => endTimeRange == default || d == endTimeRange),
            It.Is<CancellationToken>(ct => token == CancellationToken.None || ct == token)));
    }

    /// <summary>
    /// Настраивает мок для возврата категоризированных трат.
    /// <inheritdoc cref="ITransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeAsync" path="/summary"/>
    /// </summary>
    /// <param name="mockedTransactionRepository">Мок репозитория транзакций</param>
    /// <param name="result">Коллекция категоризированных трат для возврата</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="startDate">Начало периода</param>
    /// <param name="endDate">Конец периода</param>
    /// <param name="token">Токен отмены</param>
    /// <remarks>
    /// <inheritdoc cref="ITransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeAsync" path="/remarks"/>
    /// </remarks>
    public static void GetCategorizedSpendingByUserIdAndDateRangeReturnValue(
        this Mock<ITransactionRepository> mockedTransactionRepository,
        CategorizedSpendingResult result,
        Guid userId = default,
        DateTimeOffset startDate = default,
        DateTimeOffset endDate = default,
        CancellationToken token = default)
        => mockedTransactionRepository.SetupGetCategorizedSpendingByUserIdAndDateRange(userId, startDate, endDate, token)
            .ReturnsAsync(result);

    private static ISetup<ITransactionRepository, Task<CategorizedSpendingResult>> SetupGetCategorizedSpendingByUserIdAndDateRange(
		this Mock<ITransactionRepository> mockedTransactionRepository,
		Guid userId = default,
		DateTimeOffset startDate = default,
		DateTimeOffset endDate = default,
		CancellationToken token = default)
	{
		return mockedTransactionRepository.Setup(x => x.GetCategorizedSpendingByUserIdAndDateRangeAsync(
			It.Is<Guid>(g => userId == Guid.Empty || g == userId),
			It.Is<DateTimeOffset>(d => startDate == default || d == startDate),
			It.Is<DateTimeOffset>(d => endDate == default || d == endDate),
			It.Is<CancellationToken>(ct => token == CancellationToken.None || ct == token)));
	}

	/// <summary>
	/// Настраивает мок для возврата данных линейного графика трат.
	/// <inheritdoc cref="ITransactionRepository.GetSpendingTrendLineAsync" path="/summary"/>
	/// </summary>
	/// <param name="mockedTransactionRepository">Мок репозитория транзакций</param>
	/// <param name="result">Результат для возврата</param>
	/// <param name="userId">Идентификатор пользователя</param>
	/// <param name="token">Токен отмены</param>
	public static void GetSpendingTrendLineReturnValue(
		this Mock<ITransactionRepository> mockedTransactionRepository,
		SpendingTrendLineResult result,
		Guid userId = default,
		CancellationToken token = default)
		=> mockedTransactionRepository.Setup(x => x.GetSpendingTrendLineAsync(
			It.Is<Guid>(g => userId == Guid.Empty || g == userId),
			It.IsAny<IReadOnlyCollection<DateRangeInfo>>(),
			It.Is<CancellationToken>(ct => token == CancellationToken.None || ct == token)))
			.ReturnsAsync(result);
}
