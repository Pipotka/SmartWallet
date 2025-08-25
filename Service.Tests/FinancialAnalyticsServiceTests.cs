using System.Collections.Immutable;
using Ahatornn.TestGenerator;
using FluentAssertions;
using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Infrastructure;
using Nasurino.SmartWallet.Services;
using Nasurino.SmartWallet.UnitTests;
using Nasurino.SmartWallet.UnitTests.Extensions;
using Service.Infrastructure.Contracts;
using Services.Contracts;
using Xunit;

namespace Nasurino.SmartWallet.Service.Tests;

/// <summary>
/// Тесты на <see cref="FinancialAnalyticsService"/>
/// </summary>
public class FinancialAnalyticsServiceTests
{
    private readonly TestEntityProvider entityProvider;
    private readonly IFinancialCalculator calculator;
    private readonly Mock<IUserRepository> mockedUserRepository;
    private readonly Mock<ITransactionRepository> mockedTransactionRepository;
    private readonly IFinancialAnalyticsService financialAnalyticsService;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="FinancialAnalyticsServiceTests"/>
    /// </summary>
    public FinancialAnalyticsServiceTests()
    {
        entityProvider = TestEntityProvider.Shared;
        calculator = new FinancialCalculator();
        
        mockedUserRepository = new Moq.Mock<IUserRepository>();
        mockedTransactionRepository = new Moq.Mock<ITransactionRepository>();
        var unitOfWork = new MockedUnitOfWork(mockedUserRepository : mockedUserRepository,
            mockedTransactionRepository : mockedTransactionRepository);
        
        financialAnalyticsService = new FinancialAnalyticsService(unitOfWork, calculator);
    }

    /// <summary>
    /// Должен вернуть значение в процентах 
    /// </summary>
    [Fact]
    public async Task GetCategorizingSpendingByTimeRangeAndUserIdShouldReturnValueInPercentage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetSum = 10000d;
        var firstAreaPercentage = 25d;
        var secondAreaPercentage = 75d;
        var transactions = ImmutableArray.CreateRange<Transaction>([
            entityProvider.Create<Transaction>(x => 
                x.Value = calculator.PercentageOfSum(targetSum, firstAreaPercentage)),
            entityProvider.Create<Transaction>(x => 
                x.Value = calculator.PercentageOfSum(targetSum, secondAreaPercentage))
        ]);
        
        mockedUserRepository.GetUserByIdReturnNotNull(userId);
        mockedTransactionRepository.GetListByTimeRangeReturnValue(transactions, userId);

        // Act
        var result = await financialAnalyticsService.GetCategorizingSpendingByTimeRangeAndUserIdAsync(userId,
            DateTime.Now,
            DateTime.Now,
            true,
            CancellationToken.None);

        // Assert
        result.SpendingAmount.Should().Be(targetSum);
        result.CategorizedSpending[transactions.First().ToSpendingAreaId]
            .Should().Be(firstAreaPercentage);
        result.CategorizedSpending[transactions[1].ToSpendingAreaId]
            .Should().Be(secondAreaPercentage);
    }
}