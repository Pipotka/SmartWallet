using Moq;
using Moq.Language.Flow;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;

namespace Nasurino.SmartWallet.UnitTests.Services.Infrastructure.Mock.Extensions;

/// <summary>
/// Методы-расширения для моков <see cref="IDailyExpenseCategorieRepository"/>
/// </summary>
public static class DailyExpenseCategorieRepositoryMockExtensions
{
	/// <summary>
	/// Настраивает мок для возврата категоризированных трат.
	/// <inheritdoc cref="IDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeAsync" path="/summary"/>
	/// </summary>
	/// <param name="mockedRepository">Мок репозитория</param>
	/// <param name="result">Коллекция категоризированных трат для возврата</param>
	/// <param name="userId">Идентификатор пользователя</param>
	/// <param name="startDate">Начало периода</param>
	/// <param name="endDate">Конец периода</param>
	/// <param name="token">Токен отмены</param>
	/// <remarks>
	/// <inheritdoc cref="IDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeAsync" path="/remarks"/>
	/// </remarks>
	public static void GetCategorizedSpendingByUserIdAndDateRangeReturnValue(
		this Mock<IDailyExpenseCategorieRepository> mockedRepository,
		CategorizedSpendingResult result,
		Guid userId = default,
		DateTimeOffset startDate = default,
		DateTimeOffset endDate = default,
		CancellationToken token = default)
		=> mockedRepository.SetupGetCategorizedSpendingByUserIdAndDateRange(userId, startDate, endDate, token)
			.ReturnsAsync(result);

	private static ISetup<IDailyExpenseCategorieRepository, Task<CategorizedSpendingResult>> SetupGetCategorizedSpendingByUserIdAndDateRange(
		this Mock<IDailyExpenseCategorieRepository> mockedRepository,
		Guid userId = default,
		DateTimeOffset startDate = default,
		DateTimeOffset endDate = default,
		CancellationToken token = default)
	{
		return mockedRepository.Setup(x => x.GetCategorizedSpendingByUserIdAndDateRangeAsync(
			It.Is<Guid>(g => userId == Guid.Empty || g == userId),
			It.Is<DateTimeOffset>(d => startDate == default || d == startDate),
			It.Is<DateTimeOffset>(d => endDate == default || d == endDate),
			It.Is<CancellationToken>(ct => token == CancellationToken.None || ct == token)));
	}

	/// <summary>
	/// Настраивает мок для возврата данных линейного графика трат.
	/// <inheritdoc cref="IDailyExpenseCategorieRepository.GetSpendingTrendLineAsync" path="/summary"/>
	/// </summary>
	/// <param name="mockedRepository">Мок репозитория</param>
	/// <param name="result">Результат для возврата</param>
	/// <param name="userId">Идентификатор пользователя</param>
	/// <param name="token">Токен отмены</param>
	public static void GetSpendingTrendLineReturnValue(
		this Mock<IDailyExpenseCategorieRepository> mockedRepository,
		SpendingTrendLineResult result,
		Guid userId = default,
		CancellationToken token = default)
		=> mockedRepository.Setup(x => x.GetSpendingTrendLineAsync(
			It.Is<Guid>(g => userId == Guid.Empty || g == userId),
			It.IsAny<IReadOnlyCollection<DateRangeInfo>>(),
			It.Is<CancellationToken>(ct => token == CancellationToken.None || ct == token)))
			.ReturnsAsync(result);
}
