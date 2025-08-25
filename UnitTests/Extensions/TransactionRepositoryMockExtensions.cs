using Moq;
using Moq.Language.Flow;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.UnitTests.Extensions;

/// <summary>
/// Методы-расширения для моков <see cref="ITransactionRepository"/>
/// </summary>
public static class TransactionRepositoryMockExtensions
{
    /// <summary>
    /// Настраивает мок для возврата списка транзакций.
    /// <inheritdoc cref="ITransactionRepository.GetListByTimeRangeAndUserIdAsync" path="/summary"/>
    /// </summary>
    /// <param name="mockedTransactionRepository">Мок репозитория транзакций</param>
    /// <param name="transactions">Список транзакций для возврата</param>
    /// <param name="userId">ID пользователя</param>
    /// <param name="startTimeRange">Начало временного диапазона</param>
    /// <param name="endTimeRange">Конец временного диапазона</param>
    /// <param name="token">Токен отмены</param>
    /// <remarks>
    /// <inheritdoc cref="ITransactionRepository.GetListByTimeRangeAndUserIdAsync" path="/remarks"/>
    /// </remarks>
    public static void GetListByTimeRangeReturnValue(this Mock<ITransactionRepository> mockedTransactionRepository,
        IReadOnlyCollection<Transaction> transactions,
        Guid userId = default,
        DateTime startTimeRange = default,
        DateTime endTimeRange = default,
        CancellationToken token = default)
        => mockedTransactionRepository.SetupListByTimeRange(userId, startTimeRange, endTimeRange, token)
            .ReturnsAsync(transactions);

    private static ISetup<ITransactionRepository, Task<IReadOnlyCollection<Transaction>>> SetupListByTimeRange(
        this Mock<ITransactionRepository> mockedTransactionRepository,
        Guid userId = default,
        DateTime startTimeRange = default,
        DateTime endTimeRange = default,
        CancellationToken token = default)
    {
        return mockedTransactionRepository.Setup(x => x.GetListByTimeRangeAndUserIdAsync(
            It.Is<Guid>(g => userId == Guid.Empty || g == userId),
            It.Is<DateTime>(d => startTimeRange == default || d == startTimeRange),
            It.Is<DateTime>(d => endTimeRange == default || d == endTimeRange),
            It.Is<CancellationToken>(ct => token == CancellationToken.None || ct == token)));
    }
}