using System.Collections.Immutable;
using Ahatornn.TestGenerator;
using AutoMapper;
using FluentAssertions;
using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Infrastructure;
using Nasurino.SmartWallet.Services;
using Nasurino.SmartWallet.Services.AutoMappers;
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
        var mapper = new MapperConfiguration(conf => conf.AddProfile<ServiceModelMapper>()).CreateMapper();
        
        _calculator = new FinancialCalculator();
        
        _mockedUserRepository = new Mock<IUserRepository>();
        _mockedTransactionRepository = new Mock<ITransactionRepository>();
        var unitOfWork = new MockedUnitOfWork(mockedUserRepository : _mockedUserRepository,
            mockedTransactionRepository : _mockedTransactionRepository);
        
        _financialAnalyticsService = new FinancialAnalyticsService(unitOfWork, _calculator, mapper);
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
        
        var spendingItems = ImmutableArray.Create<CategorySpendingItem>([
            new CategorySpendingItem
            {
                CategoryId = firstTransaction.DestinationAccountId!.Value,
                CategoryName = string.Empty,
                TotalAmount = firstTransaction.Amount
            },
            new CategorySpendingItem
            {
                CategoryId = secondTransaction.DestinationAccountId!.Value,
                CategoryName = string.Empty,
                TotalAmount = secondTransaction.Amount
            }
        ]);
        
        var categorizedResult = new CategorizedSpendingResult
        {
            TotalSpending = targetSum,
            Categories = spendingItems
        };
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(categorizedResult, userId,
            startDate: DateTime.Today, endDate: DateTime.Today.AddDays(1));

        // Act
        var actual = await _financialAnalyticsService.GetCategorizingSpendingByDateRangeAndUserIdAsync(userId,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CancellationToken.None);

        // Assert
        actual.TotalSpending.Should().Be(targetSum);
        var item1 = actual.Categories.FirstOrDefault(x => x.CategoryId == firstTransaction.DestinationAccountId!.Value);
        item1.Should().NotBeNull();
        item1.TotalAmount.Should().Be(firstTransaction.Amount);
        var item2 = actual.Categories.FirstOrDefault(x => x.CategoryId == secondTransaction.DestinationAccountId!.Value);
        item2.Should().NotBeNull();
        item2.TotalAmount.Should().Be(secondTransaction.Amount);
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
        
        var spendingItems = ImmutableArray.Create<CategorySpendingItem>([
            new CategorySpendingItem
            {
                CategoryId = firstTransaction.DestinationAccountId!.Value,
                CategoryName = string.Empty,
                TotalAmount = firstTransaction.Amount
            },
            new CategorySpendingItem
            {
                CategoryId = secondTransaction.DestinationAccountId!.Value,
                CategoryName = string.Empty,
                TotalAmount = secondTransaction.Amount
            }
        ]);
        
        var categorizedResult = new CategorizedSpendingResult
        {
            TotalSpending = targetSum,
            Categories = spendingItems
        };
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(categorizedResult, userId,
            startDate: DateTime.Today, endDate: DateTime.Today);

        // Act
        var actual = await _financialAnalyticsService.GetCategorizingSpendingByDateRangeAndUserIdAsync(userId,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today),
            CancellationToken.None);

        // Assert
        actual.TotalSpending.Should().Be(targetSum);
        var item1 = actual.Categories.FirstOrDefault(x => x.CategoryId == firstTransaction.DestinationAccountId!.Value);
        item1.Should().NotBeNull();
        item1.TotalAmount.Should().Be(firstTransaction.Amount);
        var item2 = actual.Categories.FirstOrDefault(x => x.CategoryId == secondTransaction.DestinationAccountId!.Value);
        item2.Should().NotBeNull();
        item2.TotalAmount.Should().Be(secondTransaction.Amount);
    }
    
    /// <summary>
    /// Должен вернуть нулевое значение общей суммы и пустой список категорий
    /// </summary>
    [Fact]
    public async Task GetCategorizingSpendingByTimeRangeAndUserIdShouldReturnZeros()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var spendingItems = ImmutableArray.Create<CategorySpendingItem>();
        
        var categorizedResult = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = spendingItems
        };
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(categorizedResult, userId,
            startDate: DateTime.Today, endDate: DateTime.Today);

        // Act
        var actual = await _financialAnalyticsService.GetCategorizingSpendingByDateRangeAndUserIdAsync(userId,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today),
            CancellationToken.None);

        // Assert
        actual.TotalSpending.Should().Be(0d);
        actual.Categories.Should().BeEmpty();
    }
    
    /// <summary>
    /// Должен выбросить исключение о том что пользователь не найден
    /// </summary>
    [Fact]
    public async Task GetCategorizingSpendingByTimeRangeAndUserIdShouldThrowNotFoundUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var spendingItems = ImmutableArray.Create<CategorySpendingItem>();
        
        var categorizedResult = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = spendingItems
        };
        
        _mockedUserRepository.GetUserByIdReturnNotNull(Guid.NewGuid());
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(categorizedResult,
            startDate: DateTime.Today, endDate: DateTime.Today);

        // Act
        var act = () => _financialAnalyticsService.GetCategorizingSpendingByDateRangeAndUserIdAsync(userId,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today),
            CancellationToken.None);

        // Assert
        await act.ShouldThrowEntityNotFoundException($"*{userId}*");
    }
    
    /// <summary>
    /// Должен корректно вычислять процент роста (положительная динамика)
    /// </summary>
    [Fact]
    public void CalculateTrendPercentageShouldCalculatePositiveTrend()
    {
        // Arrange
        var currentValue = 150.0;
        var previousValue = 100.0;
        
        // Act
        var result = _calculator.CalculateTrendPercentage(currentValue, previousValue);
        
        // Assert
        result.Should().Be(50.0);
    }
    
    /// <summary>
    /// Должен корректно вычислять процент снижения (отрицательная динамика)
    /// </summary>
    [Fact]
    public void CalculateTrendPercentageShouldCalculateNegativeTrend()
    {
        // Arrange
        var currentValue = 50.0;
        var previousValue = 100.0;
        
        // Act
        var result = _calculator.CalculateTrendPercentage(currentValue, previousValue);
        
        // Assert
        result.Should().Be(-50.0);
    }
    
    /// <summary>
    /// Должен возвращать 0 при отсутствии изменений
    /// </summary>
    [Fact]
    public void CalculateTrendPercentageShouldReturnZeroWhenNoChange()
    {
        // Arrange
        var currentValue = 100.0;
        var previousValue = 100.0;
        
        // Act
        var result = _calculator.CalculateTrendPercentage(currentValue, previousValue);
        
        // Assert
        result.Should().Be(0.0);
    }
    
    /// <summary>
    /// Должен возвращать 0 когда previousValue равно 0
    /// </summary>
    [Fact]
    public void CalculateTrendPercentageShouldReturnZeroWhenPreviousValueIsZero()
    {
        // Arrange
        var currentValue = 200.0;
        var previousValue = 0.0;
        
        // Act
        var result = _calculator.CalculateTrendPercentage(currentValue, previousValue);
        
        // Assert
        result.Should().Be(0.0);
    }
    
    /// <summary>
    /// Должен выбрасывать исключение при отрицательном currentValue
    /// </summary>
    [Fact]
    public void CalculateTrendPercentageShouldThrowExceptionWhenCurrentValueIsNegative()
    {
        // Arrange
        var currentValue = -50.0;
        var previousValue = 100.0;
        
        // Act
        var act = () => _calculator.CalculateTrendPercentage(currentValue, previousValue);
        
        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("currentValue")
            .WithMessage("Текущее значение не может быть отрицательным*");
    }
    
    /// <summary>
    /// Должен выбрасывать исключение при отрицательном previousValue
    /// </summary>
    [Fact]
    public void CalculateTrendPercentageShouldThrowExceptionWhenPreviousValueIsNegative()
    {
        // Arrange
        var currentValue = 150.0;
        var previousValue = -100.0;
        
        // Act
        var act = () => _calculator.CalculateTrendPercentage(currentValue, previousValue);
        
        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("previousValue")
            .WithMessage("Предыдущее значение не может быть отрицательным*");
    }
    
    /// <summary>
    /// Должен корректно округлять результат до указанного количества знаков после запятой
    /// </summary>
    [Fact]
    public void CalculateTrendPercentageShouldRoundResultToSpecifiedDecimals()
    {
        // Arrange
        var currentValue = 123.456;
        var previousValue = 100.0;
        
        // Act
        var result = _calculator.CalculateTrendPercentage(currentValue, previousValue, decimals: 3);
        
        // Assert
        result.Should().Be(23.456);
    }
    
    /// <summary>
    /// Должен возвращать 0 по умолчанию (2 знака после запятой)
    /// </summary>
    [Fact]
    public void CalculateTrendPercentageShouldReturnZeroWithDefaultDecimals()
    {
        // Arrange
        var currentValue = 200.0;
        var previousValue = 0.0;
        
        // Act
        var result = _calculator.CalculateTrendPercentage(currentValue, previousValue);
        
        // Assert
        result.Should().Be(0.0);
    }
}