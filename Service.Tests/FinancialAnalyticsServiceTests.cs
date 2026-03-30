using System.Collections.Immutable;
using Ahatornn.TestGenerator;
using AutoMapper;
using FluentAssertions;
using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Infrastructure;
using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;
using Nasurino.SmartWallet.Services;
using Nasurino.SmartWallet.Services.AutoMappers;
using Nasurino.SmartWallet.Services.Contracts;
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
    private readonly Mock<ISmartWalletValidateService> _validateServiceMock;
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
        // TODO Написать тесты на проверку валидатора
        _validateServiceMock = new Mock<ISmartWalletValidateService>();
        
        _mockedUserRepository = new Mock<IUserRepository>();
        _mockedTransactionRepository = new Mock<ITransactionRepository>();
        var unitOfWork = new MockedUnitOfWork(mockedUserRepository : _mockedUserRepository,
            mockedTransactionRepository : _mockedTransactionRepository);
        
        _financialAnalyticsService = new FinancialAnalyticsService(unitOfWork,
            _calculator,
            _validateServiceMock.Object,
            mapper);
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
    
    #region GetSpendingTrendAnalysisAsync Tests
    
    /// <summary>
    /// TC-ERR-01: Должен выбросить исключение когда пользователь не найден
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SpendingTrendAnalysisRequest
        {
            UserId = userId,
            FirstDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            SecondDate = DateOnly.FromDateTime(DateTime.Today),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var act = () => _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        await act.ShouldThrowEntityNotFoundException($"*{userId}*");
    }
    
    /// <summary>
    /// TC-BAS-01: Оба периода пустые (0 трат)
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWhenBothPeriodsEmptyShouldReturnZeroValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SpendingTrendAnalysisRequest
        {
            UserId = userId,
            FirstDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        var emptyResult = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = []
        };
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(emptyResult, userId,
            startDate: It.IsAny<DateTime>(), endDate: It.IsAny<DateTime>());
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalCurrentSpending.Should().Be(0);
        actual.TotalPreviousSpending.Should().Be(0);
        actual.TotalSpendingTrendPercentage.Should().Be(0);
        actual.CategoryTrends.Should().BeEmpty();
    }
    
    /// <summary>
    /// TC-BAS-02: Только текущий период имеет траты, предыдущий пуст
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWhenOnlyCurrentPeriodHasSpendingShouldReturnZeroTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SpendingTrendAnalysisRequest
        {
            UserId = userId,
            FirstDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        
        var previousEmpty = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = ImmutableArray.Create<CategorySpendingItem>()
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1000,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId1, CategoryName = "Food", TotalAmount = 600 },
                new CategorySpendingItem { CategoryId = categoryId2, CategoryName = "Transport", TotalAmount = 400 }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousEmpty, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalCurrentSpending.Should().Be(1000);
        actual.TotalPreviousSpending.Should().Be(0);
        actual.TotalSpendingTrendPercentage.Should().Be(0);
        actual.CategoryTrends.Should().HaveCount(2);
        
        var foodTrend = actual.CategoryTrends.First(x => x.CategoryName == "Food");
        foodTrend.CurrentPeriodAmount.Should().Be(600);
        foodTrend.TrendPercentage.Should().Be(0);
        
        var transportTrend = actual.CategoryTrends.First(x => x.CategoryName == "Transport");
        transportTrend.CurrentPeriodAmount.Should().Be(400);
        transportTrend.TrendPercentage.Should().Be(0);
    }
    
    /// <summary>
    /// TC-BAS-03: Только предыдущий период имеет траты, текущий пуст
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWhenOnlyPreviousPeriodHasSpendingShouldReturnNegativeHundredTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SpendingTrendAnalysisRequest
        {
            UserId = userId,
            FirstDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1000,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId1, CategoryName = "Food", TotalAmount = 600 },
                new CategorySpendingItem { CategoryId = categoryId2, CategoryName = "Transport", TotalAmount = 400 }
            ])
        };
        
        var currentEmpty = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = ImmutableArray.Create<CategorySpendingItem>()
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentEmpty, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalCurrentSpending.Should().Be(0);
        actual.TotalPreviousSpending.Should().Be(1000);
        actual.TotalSpendingTrendPercentage.Should().Be(-100);
        actual.CategoryTrends.Should().HaveCount(2);
        
        var foodTrend = actual.CategoryTrends.First(x => x.CategoryName == "Food");
        foodTrend.CurrentPeriodAmount.Should().Be(0);
        foodTrend.TrendPercentage.Should().Be(-100);
        
        var transportTrend = actual.CategoryTrends.First(x => x.CategoryName == "Transport");
        transportTrend.CurrentPeriodAmount.Should().Be(0);
        transportTrend.TrendPercentage.Should().Be(-100);
    }
    
    /// <summary>
    /// TC-BAS-04: Оба периода имеют одинаковые траты
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWhenBothPeriodsHaveEqualSpendingShouldReturnZeroTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SpendingTrendAnalysisRequest
        {
            UserId = userId,
            FirstDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1000,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 1000 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1000,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 1000 }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalCurrentSpending.Should().Be(1000);
        actual.TotalPreviousSpending.Should().Be(1000);
        actual.TotalSpendingTrendPercentage.Should().Be(0);
        actual.CategoryTrends.Should().HaveCount(1);
        
        var foodTrend = actual.CategoryTrends.First();
        foodTrend.CurrentPeriodAmount.Should().Be(1000);
        foodTrend.TrendPercentage.Should().Be(0);
    }
    
    /// <summary>
    /// TC-CAT-01: Одна категория в обоих периодах с разными суммами
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithOneCategoryInBothPeriodsShouldCalculateCorrectTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SpendingTrendAnalysisRequest
        {
            UserId = userId,
            FirstDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 500,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 500 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 750,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 750 }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalCurrentSpending.Should().Be(750);
        actual.TotalPreviousSpending.Should().Be(500);
        actual.TotalSpendingTrendPercentage.Should().Be(50); // (750-500)/500 * 100 = 50%
        actual.CategoryTrends.Should().HaveCount(1);
        
        var foodTrend = actual.CategoryTrends.First();
        foodTrend.CategoryId.Should().Be(categoryId);
        foodTrend.CategoryName.Should().Be("Food");
        foodTrend.CurrentPeriodAmount.Should().Be(750);
        foodTrend.TrendPercentage.Should().Be(50);
    }
    
    /// <summary>
    /// TC-CAT-02: Несколько категорий в обоих периодах
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithMultipleCategoriesInBothPeriodsShouldCalculateAllTrends()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SpendingTrendAnalysisRequest
        {
            UserId = userId,
            FirstDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        var foodId = Guid.NewGuid();
        var transportId = Guid.NewGuid();
        var entertainmentId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1000,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = foodId, CategoryName = "Food", TotalAmount = 500 },
                new CategorySpendingItem { CategoryId = transportId, CategoryName = "Transport", TotalAmount = 300 },
                new CategorySpendingItem { CategoryId = entertainmentId, CategoryName = "Entertainment", TotalAmount = 200 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1200,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = foodId, CategoryName = "Food", TotalAmount = 600 },
                new CategorySpendingItem { CategoryId = transportId, CategoryName = "Transport", TotalAmount = 250 },
                new CategorySpendingItem { CategoryId = entertainmentId, CategoryName = "Entertainment", TotalAmount = 350 }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalCurrentSpending.Should().Be(1200);
        actual.TotalPreviousSpending.Should().Be(1000);
        actual.TotalSpendingTrendPercentage.Should().Be(20);
        actual.CategoryTrends.Should().HaveCount(3);
        
        var foodTrend = actual.CategoryTrends.First(x => x.CategoryName == "Food");
        foodTrend.CurrentPeriodAmount.Should().Be(600);
        foodTrend.TrendPercentage.Should().Be(20); // (600-500)/500 * 100 = 20%
        
        var transportTrend = actual.CategoryTrends.First(x => x.CategoryName == "Transport");
        transportTrend.CurrentPeriodAmount.Should().Be(250);
        transportTrend.TrendPercentage.Should().Be(-16.67); // (250-300)/300 * 100 ≈ -16.67%
        
        var entertainmentTrend = actual.CategoryTrends.First(x => x.CategoryName == "Entertainment");
        entertainmentTrend.CurrentPeriodAmount.Should().Be(350);
        entertainmentTrend.TrendPercentage.Should().Be(75); // (350-200)/200 * 100 = 75%
    }
    
    /// <summary>
    /// TC-CAT-03: Категория с ростом (current > previous) - положительный процент
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithCategoryGrowthShouldReturnPositiveTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 300,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 300 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 450,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 450 }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        var trend = actual.CategoryTrends.First();
        trend.TrendPercentage.Should().BePositive();
        trend.TrendPercentage.Should().Be(50); // (450-300)/300 * 100 = 50%
    }
    
    /// <summary>
    /// TC-CAT-04: Категория с падением (current < previous) - отрицательный процент
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithCategoryDeclineShouldReturnNegativeTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 500,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 500 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 300,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 300 }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        var trend = actual.CategoryTrends.First();
        trend.TrendPercentage.Should().BeNegative();
        trend.TrendPercentage.Should().Be(-40); // (300-500)/500 * 100 = -40%
    }
    
    /// <summary>
    /// TC-CAT-05: Категория без изменений (current == previous) - 0%
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithCategoryNoChangeShouldReturnZeroTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        var amount = 500.0;
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = amount,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = amount }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = amount,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = amount }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        var trend = actual.CategoryTrends.First();
        trend.TrendPercentage.Should().Be(0);
        trend.CurrentPeriodAmount.Should().Be(amount);
    }
    
    /// <summary>
    /// TC-CAT-06: Previous = 0, Current > 0 - тренд 0% (защита от деления на ноль)
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWhenPreviousCategoryAmountIsZeroShouldReturnZeroTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 0 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 500,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 500 }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        var trend = actual.CategoryTrends.First();
        trend.CurrentPeriodAmount.Should().Be(500);
        trend.TrendPercentage.Should().Be(0); // Защита от деления на ноль
    }
    
    /// <summary>
    /// TC-CAT-07: Previous > 0, Current = 0 - отрицательный тренд (например, -100%)
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWhenCurrentCategoryAmountIsZeroShouldReturnNegativeTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 500,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 500 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = ImmutableArray.Create<CategorySpendingItem>()
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        var trend = actual.CategoryTrends.First();
        trend.CurrentPeriodAmount.Should().Be(0);
        trend.TrendPercentage.Should().Be(-100); // (0-500)/500 * 100 = -100%
    }
    
    /// <summary>
    /// TC-NEW-01: Одна новая категория в текущем периоде
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithNewCategoryInCurrentPeriodShouldReturnZeroTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 500,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = Guid.NewGuid(), CategoryName = "Food", TotalAmount = 500 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 800,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Entertainment", TotalAmount = 300 }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.CategoryTrends.Should().HaveCount(2);
        
        var newCategoryTrend = actual.CategoryTrends.First(x => x.CategoryName == "Entertainment");
        newCategoryTrend.CategoryId.Should().Be(categoryId);
        newCategoryTrend.CurrentPeriodAmount.Should().Be(300);
        newCategoryTrend.TrendPercentage.Should().Be(0); // Новая категория - тренд 0%
    }
    
    /// <summary>
    /// TC-NEW-02: Несколько новых категорий в текущем периоде
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithMultipleNewCategoriesShouldAllHaveZeroTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 500,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = Guid.NewGuid(), CategoryName = "Food", TotalAmount = 500 }
            ])
        };
        
        var newCategory1Id = Guid.NewGuid();
        var newCategory2Id = Guid.NewGuid();
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1200,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = newCategory1Id, CategoryName = "Entertainment", TotalAmount = 400 },
                new CategorySpendingItem { CategoryId = newCategory2Id, CategoryName = "Shopping", TotalAmount = 300 }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.CategoryTrends.Should().HaveCount(3);
        
        var entertainmentTrend = actual.CategoryTrends.First(x => x.CategoryName == "Entertainment");
        entertainmentTrend.TrendPercentage.Should().Be(0);
        entertainmentTrend.CurrentPeriodAmount.Should().Be(400);
        
        var shoppingTrend = actual.CategoryTrends.First(x => x.CategoryName == "Shopping");
        shoppingTrend.TrendPercentage.Should().Be(0);
        shoppingTrend.CurrentPeriodAmount.Should().Be(300);
    }
    
    /// <summary>
    /// TC-NEW-03: Комбинация - есть в обоих периодах + новые категории
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithMixedCategoriesIncludingNewShouldHandleCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var commonCategoryId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 800,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = commonCategoryId, CategoryName = "Food", TotalAmount = 500 },
                new CategorySpendingItem { CategoryId = Guid.NewGuid(), CategoryName = "Transport", TotalAmount = 300 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1100,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = commonCategoryId, CategoryName = "Food", TotalAmount = 600 },
                new CategorySpendingItem { CategoryId = newCategoryId, CategoryName = "Entertainment", TotalAmount = 500 }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.CategoryTrends.Should().HaveCount(3);
        
        var commonTrend = actual.CategoryTrends.First(x => x.CategoryName == "Food");
        commonTrend.TrendPercentage.Should().Be(20); // (600-500)/500 * 100 = 20%
        commonTrend.CurrentPeriodAmount.Should().Be(600);
        
        var newTrend = actual.CategoryTrends.First(x => x.CategoryName == "Entertainment");
        newTrend.TrendPercentage.Should().Be(0);
        newTrend.CurrentPeriodAmount.Should().Be(500);
        
        var transportTrend = actual.CategoryTrends.First(x => x.CategoryName == "Transport");
        transportTrend.TrendPercentage.Should().Be(-100);
        transportTrend.CurrentPeriodAmount.Should().Be(0);
    }
    
    /// <summary>
    /// TC-LOST-01: Одна исчезнувшая категория (есть только в предыдущем периоде)
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithLostCategoryShouldReturnNegativeHundredTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var lostCategoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 800,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = lostCategoryId, CategoryName = "Food", TotalAmount = 800 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = ImmutableArray.Create<CategorySpendingItem>()
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.CategoryTrends.Should().HaveCount(1);
        
        var lostTrend = actual.CategoryTrends.First();
        lostTrend.CategoryId.Should().Be(lostCategoryId);
        lostTrend.CategoryName.Should().Be("Food");
        lostTrend.CurrentPeriodAmount.Should().Be(0);
        lostTrend.TrendPercentage.Should().Be(-100);
    }
    
    /// <summary>
    /// TC-LOST-02: Несколько исчезнувших категорий
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithMultipleLostCategoriesShouldAllHaveNegativeHundredTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1000,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = Guid.NewGuid(), CategoryName = "Food", TotalAmount = 600 },
                new CategorySpendingItem { CategoryId = Guid.NewGuid(), CategoryName = "Transport", TotalAmount = 400 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = ImmutableArray.Create<CategorySpendingItem>()
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.CategoryTrends.Should().HaveCount(2);
        
        foreach (var trend in actual.CategoryTrends)
        {
            trend.CurrentPeriodAmount.Should().Be(0);
            trend.TrendPercentage.Should().Be(-100);
        }
    }
    
    /// <summary>
    /// TC-LOST-03: Комбинация - есть в обоих + исчезнувшие категории
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithMixedCategoriesIncludingLostShouldHandleCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var commonCategoryId = Guid.NewGuid();
        var lostCategoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1200,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = commonCategoryId, CategoryName = "Food", TotalAmount = 500 },
                new CategorySpendingItem { CategoryId = lostCategoryId, CategoryName = "Transport", TotalAmount = 700 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 600,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = commonCategoryId, CategoryName = "Food", TotalAmount = 600 }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.CategoryTrends.Should().HaveCount(2);
        
        var commonTrend = actual.CategoryTrends.First(x => x.CategoryName == "Food");
        commonTrend.TrendPercentage.Should().Be(20); // (600-500)/500 * 100 = 20%
        commonTrend.CurrentPeriodAmount.Should().Be(600);
        
        var lostTrend = actual.CategoryTrends.First(x => x.CategoryName == "Transport");
        lostTrend.TrendPercentage.Should().Be(-100);
        lostTrend.CurrentPeriodAmount.Should().Be(0);
    }
    
    /// <summary>
    /// TC-COM-04: Категории с дробными суммами - проверка округления
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithFractionalAmountsShouldRoundCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 123.456,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 123.456 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 234.567,
            Categories = ImmutableArray.Create<CategorySpendingItem>([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 234.567 }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        _mockedTransactionRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            endDate: currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalSpendingTrendPercentage.Should().Be(90.00); 
        
        var trend = actual.CategoryTrends.First();
        trend.CurrentPeriodAmount.Should().Be(234.567);
        trend.TrendPercentage.Should().Be(90.00);
    }
    
    /// <summary>
    /// TC-TIME-01: TimeUnit = Day - корректное вычисление диапазонов
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendAnalysisWithTimeUnitDayShouldCalculateCorrectDateRanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var firstDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));
        var secondDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));
        
        var request = new SpendingTrendAnalysisRequest
        {
            UserId = userId,
            FirstDate = firstDate,
            SecondDate = secondDate,
            TimeUnit = TimeUnit.Day,
            TimeUnitCount = 5
        };
        
        var emptyResult = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = ImmutableArray.Create<CategorySpendingItem>()
        };
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        
        // Настраиваем моки с проверкой дат
        _mockedTransactionRepository
            .Setup(x => x.GetCategorizedSpendingByUserIdAndDateRangeAsync(userId,
                firstDate.AddDays(-5).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                firstDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);
            
        _mockedTransactionRepository
            .Setup(x => x.GetCategorizedSpendingByUserIdAndDateRangeAsync(userId,
                secondDate.AddDays(-5).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                secondDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);
        
        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.Should().NotBeNull();
    }
    #endregion
    
    #region Helper Methods
    
    private static SpendingTrendAnalysisRequest CreateDefaultRequest(Guid userId)
    {
        return new SpendingTrendAnalysisRequest
        {
            UserId = userId,
            FirstDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
    }
    #endregion
}