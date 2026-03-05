using System.Collections.Immutable;
using Ahatornn.TestGenerator;
using FluentAssertions;
using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Infrastructure;
using Nasurino.SmartWallet.Services;
using Nasurino.SmartWallet.UnitTests.Services.FluentAssertions.Shortcuts.Extensions;
using Nasurino.SmartWallet.UnitTests.Services.Infrastructure;
using Nasurino.SmartWallet.UnitTests.Services.Infrastructure.Mock.Extensions;
using Service.Infrastructure.Contracts;
using Services.Contracts;
using Xunit;

namespace Nasurino.SmartWallet.Service.Tests;

/// <summary>
/// Тесты на <see cref="FinancialAnalyticsService"/>
/// </summary>
public sealed class FinancialAnalyticsServiceTests
{
    private readonly TestEntityProvider _entityProvider;
    private readonly IFinancialCalculator _calculator;
    private readonly Mock<IUserRepository> _mockedUserRepository;
    private readonly Mock<ITransactionRepository> _mockedTransactionRepository;
    private readonly IFinancialAnalyticsService _financialAnalyticsService;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="FinancialAnalyticsServiceTests"/>
    /// </summary>
    public FinancialAnalyticsServiceTests()
    {
        _entityProvider = new TestEntityProviderBuilder()
            .AddPreset<Transaction>(x =>
            {
                x.DestinationAccountId = Guid.NewGuid();
                x.SourceAccountId = Guid.NewGuid();
            }).Build();
        
        _calculator = new FinancialCalculator();
        
        _mockedUserRepository = new Mock<IUserRepository>();
        _mockedTransactionRepository = new Mock<ITransactionRepository>();
        var unitOfWork = new MockedUnitOfWork(mockedUserRepository : _mockedUserRepository,
            mockedTransactionRepository : _mockedTransactionRepository);
        
        _financialAnalyticsService = new FinancialAnalyticsService(unitOfWork, _calculator);
    }

    /// <summary>
    /// Должен вернуть значение в процентах 
    /// </summary>
    [Fact]
    public async Task GetCategorizingSpendingByTimeRangeAndUserIdShouldReturnValueInPercentage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetSum = 10_000d;
        var firstAreaPercentage = 25d;
        var secondAreaPercentage = 75d;
        var transactions = ImmutableArray.CreateRange<Transaction>([
            _entityProvider.Create<Transaction>(x => 
                x.Amount = _calculator.PercentageOfSum(targetSum, firstAreaPercentage)),
            _entityProvider.Create<Transaction>(x => 
                x.Amount = _calculator.PercentageOfSum(targetSum, secondAreaPercentage))
        ]);
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _mockedTransactionRepository.GetTypedListByTimeRangeReturnValue(transactions, userId, TransactionType.Expense);

        // Act
        var result = await _financialAnalyticsService.GetCategorizingSpendingByDateRangeAndUserIdAsync(userId,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today),
            true,
            CancellationToken.None);

        // Assert
        result.SpendingAmount.Should().Be(targetSum);
        result.CategorizedSpending[transactions.First().DestinationAccountId!.Value]
            .Should().Be(firstAreaPercentage);
        result.CategorizedSpending[transactions[1].DestinationAccountId!.Value]
            .Should().Be(secondAreaPercentage);
    }
    
    /// <summary>
    /// Должен вернуть значение
    /// </summary>
    [Fact]
    public async Task GetCategorizingSpendingByTimeRangeAndUserIdShouldReturnValue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetSum = 10_000d;
        var firstTransaction = _entityProvider.Create<Transaction>(x => 
            x.Amount = _calculator.PercentageOfSum(targetSum, 25d));
        var secondTransaction = _entityProvider.Create<Transaction>(x => 
            x.Amount = _calculator.PercentageOfSum(targetSum, 75d));
        var transactions = ImmutableArray.CreateRange<Transaction>([
            firstTransaction,
            secondTransaction
        ]);
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _mockedTransactionRepository.GetTypedListByTimeRangeReturnValue(transactions, userId, TransactionType.Expense);

        // Act
        var result = await _financialAnalyticsService.GetCategorizingSpendingByDateRangeAndUserIdAsync(userId,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today),
            false,
            CancellationToken.None);

        // Assert
        result.SpendingAmount.Should().Be(targetSum);
        result.CategorizedSpending[firstTransaction.DestinationAccountId!.Value]
            .Should().Be(firstTransaction.Amount);
        result.CategorizedSpending[secondTransaction.DestinationAccountId!.Value]
            .Should().Be(secondTransaction.Amount);
    }
    
    /// <summary>
    /// Должен вернуть значение для сегодняшнего дня
    /// </summary>
    [Fact]
    public async Task GetCategorizingSpendingByTimeRangeAndUserIdShouldReturnValueForToday()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetSum = 10_000d;
        var firstTransaction = _entityProvider.Create<Transaction>(x =>
            {
                x.Amount = _calculator.PercentageOfSum(targetSum, 25d);
                x.MadeAt = DateTime.Today;
            });
        var secondTransaction = _entityProvider.Create<Transaction>(x =>
            {
                x.Amount = _calculator.PercentageOfSum(targetSum, 75d);
                x.MadeAt = DateTime.Today;
            });
        var transactions = ImmutableArray.CreateRange<Transaction>([
            firstTransaction,
            secondTransaction
        ]);
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _mockedTransactionRepository.GetTypedListByTimeRangeReturnValue(transactions, userId, TransactionType.Expense);

        // Act
        var result = await _financialAnalyticsService.GetCategorizingSpendingByDateRangeAndUserIdAsync(userId,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            false,
            CancellationToken.None);

        // Assert
        result.SpendingAmount.Should().Be(targetSum);
        result.CategorizedSpending[firstTransaction.DestinationAccountId!.Value]
            .Should().Be(firstTransaction.Amount);
        result.CategorizedSpending[secondTransaction.DestinationAccountId!.Value]
            .Should().Be(secondTransaction.Amount);
    }
    
    /// <summary>
    /// Должен вернуть нулевое значение общей суммы и пустой список категорий
    /// </summary>
    [Fact]
    public async Task GetCategorizingSpendingByTimeRangeAndUserIdShouldReturnZeros()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactions = ImmutableArray.Create<Transaction>();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _mockedTransactionRepository.GetTypedListByTimeRangeReturnValue(transactions, userId, TransactionType.Expense);

        // Act
        var result = await _financialAnalyticsService.GetCategorizingSpendingByDateRangeAndUserIdAsync(userId,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today),
            false,
            CancellationToken.None);

        // Assert
        result.SpendingAmount.Should().Be(0d);
        result.CategorizedSpending.Should().BeEmpty();
    }
    
    /// <summary>
    /// Должен выбросить исключение о том что пользователь не найден
    /// </summary>
    [Fact]
    public async Task GetCategorizingSpendingByTimeRangeAndUserIdShouldThrowNotFoundUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactions = ImmutableArray.Create<Transaction>();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(Guid.NewGuid());
        _mockedTransactionRepository.GetTypedListByTimeRangeReturnValue(transactions, transactionType: TransactionType.Expense);

        // Act
        var act = () => _financialAnalyticsService.GetCategorizingSpendingByDateRangeAndUserIdAsync(userId,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today),
            false,
            CancellationToken.None);

        // Assert
        await act.ShouldThrowEntityNotFoundException($"*{userId}*");
    }
}