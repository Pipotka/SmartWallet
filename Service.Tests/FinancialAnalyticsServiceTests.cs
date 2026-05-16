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
        
        var spendingItems = ImmutableArray.Create([
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

        var request = new CategorizingSpendingRequest 
        { 
            UserId = userId,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        // Act
        var actual = await _financialAnalyticsService.GetCategorizingSpendingAsync(request, CancellationToken.None);

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
        
        var spendingItems = ImmutableArray.Create([
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

        var request = new CategorizingSpendingRequest
        {
            UserId = userId,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today)
        };

        // Act
        var actual = await _financialAnalyticsService.GetCategorizingSpendingAsync(request, CancellationToken.None);

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

        var request = new CategorizingSpendingRequest
        {
            UserId = userId,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today)
        };

        // Act
        var actual = await _financialAnalyticsService.GetCategorizingSpendingAsync(request, CancellationToken.None);

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

        var request = new CategorizingSpendingRequest
        {
            UserId = userId,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        // Act
        var act = () => _financialAnalyticsService.GetCategorizingSpendingAsync(request, CancellationToken.None);

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
    
    #region CategoryComparativeAnalysisAsync Tests
    
    /// <summary>
    /// TC-ERR-01: Должен выбросить исключение когда пользователь не найден
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CategoryComparativeAnalysisRequest
        {
            UserId = userId,
            FirstPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            SecondPeriod = DateOnly.FromDateTime(DateTime.Today),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var act = () => _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        await act.ShouldThrowEntityNotFoundException($"*{userId}*");
    }
    
    /// <summary>
    /// TC-BAS-01: Оба периода пустые (0 трат)
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWhenBothPeriodsEmptyShouldReturnZeroValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CategoryComparativeAnalysisRequest
        {
            UserId = userId,
            FirstPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalSecondPeriodSpending.Should().Be(0);
        actual.TotalFirstPeriodSpending.Should().Be(0);
        actual.CategoryComparativeAnalyses.Should().BeEmpty();
    }
    
    /// <summary>
    /// TC-BAS-02: Только текущий период имеет траты, предыдущий пуст
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWhenOnlyCurrentPeriodHasSpendingShouldReturnZeroTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CategoryComparativeAnalysisRequest
        {
            UserId = userId,
            FirstPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        
        var previousEmpty = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = []
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1000,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalSecondPeriodSpending.Should().Be(1000);
        actual.TotalFirstPeriodSpending.Should().Be(0);
        actual.CategoryComparativeAnalyses.Should().HaveCount(2);
        
        var foodTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Food");
        foodTrend.SecondPeriodAmount.Should().Be(600);
        foodTrend.FirstPeriodAmount.Should().Be(0);
        
        var transportTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Transport");
        transportTrend.SecondPeriodAmount.Should().Be(400);
        transportTrend.FirstPeriodAmount.Should().Be(0);
    }
    
    /// <summary>
    /// TC-BAS-03: Только предыдущий период имеет траты, текущий пуст
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWhenOnlyPreviousPeriodHasSpendingShouldReturnNegativeHundredTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CategoryComparativeAnalysisRequest
        {
            UserId = userId,
            FirstPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1000,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = categoryId1, CategoryName = "Food", TotalAmount = 600 },
                new CategorySpendingItem { CategoryId = categoryId2, CategoryName = "Transport", TotalAmount = 400 }
            ])
        };
        
        var currentEmpty = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = []
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalSecondPeriodSpending.Should().Be(0);
        actual.TotalFirstPeriodSpending.Should().Be(1000);
        actual.CategoryComparativeAnalyses.Should().HaveCount(2);
        
        var foodTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Food");
        foodTrend.SecondPeriodAmount.Should().Be(0);
        foodTrend.FirstPeriodAmount.Should().Be(600);
        
        var transportTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Transport");
        transportTrend.SecondPeriodAmount.Should().Be(0);
        transportTrend.FirstPeriodAmount.Should().Be(400);
    }
    
    /// <summary>
    /// TC-BAS-04: Оба периода имеют одинаковые траты
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWhenBothPeriodsHaveEqualSpendingShouldReturnZeroTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CategoryComparativeAnalysisRequest
        {
            UserId = userId,
            FirstPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1000,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 1000 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1000,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalSecondPeriodSpending.Should().Be(1000);
        actual.TotalFirstPeriodSpending.Should().Be(1000);
        actual.CategoryComparativeAnalyses.Should().HaveCount(1);
        
        var foodTrend = actual.CategoryComparativeAnalyses.First();
        foodTrend.SecondPeriodAmount.Should().Be(1000);
        foodTrend.FirstPeriodAmount.Should().Be(1000);
    }
    
    /// <summary>
    /// TC-CAT-01: Одна категория в обоих периодах с разными суммами
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithOneCategoryInBothPeriodsShouldCalculateCorrectTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CategoryComparativeAnalysisRequest
        {
            UserId = userId,
            FirstPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 500,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 500 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 750,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalSecondPeriodSpending.Should().Be(750);
        actual.TotalFirstPeriodSpending.Should().Be(500);
        actual.CategoryComparativeAnalyses.Should().HaveCount(1);
        
        var foodTrend = actual.CategoryComparativeAnalyses.First();
        foodTrend.CategoryId.Should().Be(categoryId);
        foodTrend.CategoryName.Should().Be("Food");
        foodTrend.SecondPeriodAmount.Should().Be(750);
        foodTrend.FirstPeriodAmount.Should().Be(500);
    }
    
    /// <summary>
    /// TC-CAT-02: Несколько категорий в обоих периодах
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithMultipleCategoriesInBothPeriodsShouldCalculateAllTrends()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CategoryComparativeAnalysisRequest
        {
            UserId = userId,
            FirstPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
        
        var foodId = Guid.NewGuid();
        var transportId = Guid.NewGuid();
        var entertainmentId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1000,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = foodId, CategoryName = "Food", TotalAmount = 500 },
                new CategorySpendingItem { CategoryId = transportId, CategoryName = "Transport", TotalAmount = 300 },
                new CategorySpendingItem { CategoryId = entertainmentId, CategoryName = "Entertainment", TotalAmount = 200 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1200,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.TotalSecondPeriodSpending.Should().Be(1200);
        actual.TotalFirstPeriodSpending.Should().Be(1000);
        actual.CategoryComparativeAnalyses.Should().HaveCount(3);
        
        var foodTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Food");
        foodTrend.SecondPeriodAmount.Should().Be(600);
        foodTrend.FirstPeriodAmount.Should().Be(500);
        
        var transportTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Transport");
        transportTrend.SecondPeriodAmount.Should().Be(250);
        transportTrend.FirstPeriodAmount.Should().Be(300);
        
        var entertainmentTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Entertainment");
        entertainmentTrend.SecondPeriodAmount.Should().Be(350);
        entertainmentTrend.FirstPeriodAmount.Should().Be(200);
    }
    
    /// <summary>
    /// TC-CAT-03: Категория с ростом (current > previous) - положительный процент
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithCategoryGrowthShouldReturnPositiveTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 300,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 300 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 450,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        var trend = actual.CategoryComparativeAnalyses.First();
        trend.FirstPeriodAmount.Should().Be(300);
        trend.SecondPeriodAmount.Should().Be(450);
    }
    
    /// <summary>
    /// TC-CAT-04: Категория с падением (current < previous) - отрицательный процент
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithCategoryDeclineShouldReturnNegativeTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 500,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 500 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 300,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        var trend = actual.CategoryComparativeAnalyses.First();
        trend.FirstPeriodAmount.Should().Be(500);
        trend.SecondPeriodAmount.Should().Be(300);
    }
    
    /// <summary>
    /// TC-CAT-05: Категория без изменений (current == previous) - 0%
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithCategoryNoChangeShouldReturnZeroTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        var amount = 500.0;
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = amount,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = amount }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = amount,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        var trend = actual.CategoryComparativeAnalyses.First();
        trend.FirstPeriodAmount.Should().Be(amount);
        trend.SecondPeriodAmount.Should().Be(amount);
    }
    
    /// <summary>
    /// TC-CAT-06: Previous = 0, Current > 0 - тренд 0% (защита от деления на ноль)
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWhenPreviousCategoryAmountIsZeroShouldReturnZeroTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 0 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 500,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        var trend = actual.CategoryComparativeAnalyses.First();
        trend.SecondPeriodAmount.Should().Be(500);
        trend.FirstPeriodAmount.Should().Be(0);
    }
    
    /// <summary>
    /// TC-CAT-07: Previous > 0, Current = 0 - отрицательный тренд (например, -100%)
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWhenCurrentCategoryAmountIsZeroShouldReturnNegativeTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 500,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 500 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = []
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        var trend = actual.CategoryComparativeAnalyses.First();
        trend.SecondPeriodAmount.Should().Be(0);
        trend.FirstPeriodAmount.Should().Be(500);
    }
    
    /// <summary>
    /// TC-NEW-01: Одна новая категория в текущем периоде
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithNewCategoryInCurrentPeriodShouldReturnZeroTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 500,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = Guid.NewGuid(), CategoryName = "Food", TotalAmount = 500 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 800,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.CategoryComparativeAnalyses.Should().HaveCount(2);
        
        var newCategoryTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Entertainment");
        newCategoryTrend.CategoryId.Should().Be(categoryId);
        newCategoryTrend.SecondPeriodAmount.Should().Be(300);
        newCategoryTrend.FirstPeriodAmount.Should().Be(0);
    }
    
    /// <summary>
    /// TC-NEW-02: Несколько новых категорий в текущем периоде
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithMultipleNewCategoriesShouldAllHaveZeroTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 500,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = Guid.NewGuid(), CategoryName = "Food", TotalAmount = 500 }
            ])
        };
        
        var newCategory1Id = Guid.NewGuid();
        var newCategory2Id = Guid.NewGuid();
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1200,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.CategoryComparativeAnalyses.Should().HaveCount(3);
        
        var entertainmentTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Entertainment");
        entertainmentTrend.FirstPeriodAmount.Should().Be(0);
        entertainmentTrend.SecondPeriodAmount.Should().Be(400);
        
        var shoppingTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Shopping");
        shoppingTrend.FirstPeriodAmount.Should().Be(0);
        shoppingTrend.SecondPeriodAmount.Should().Be(300);
    }
    
    /// <summary>
    /// TC-NEW-03: Комбинация - есть в обоих периодах + новые категории
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithMixedCategoriesIncludingNewShouldHandleCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var commonCategoryId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 800,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = commonCategoryId, CategoryName = "Food", TotalAmount = 500 },
                new CategorySpendingItem { CategoryId = Guid.NewGuid(), CategoryName = "Transport", TotalAmount = 300 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1100,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.CategoryComparativeAnalyses.Should().HaveCount(3);
        
        var commonTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Food");
        commonTrend.FirstPeriodAmount.Should().Be(500);
        commonTrend.SecondPeriodAmount.Should().Be(600);
        
        var newTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Entertainment");
        newTrend.FirstPeriodAmount.Should().Be(0);
        newTrend.SecondPeriodAmount.Should().Be(500);
        
        var transportTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Transport");
        transportTrend.FirstPeriodAmount.Should().Be(300);
        transportTrend.SecondPeriodAmount.Should().Be(0);
    }
    
    /// <summary>
    /// TC-LOST-01: Одна исчезнувшая категория (есть только в предыдущем периоде)
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithLostCategoryShouldReturnNegativeHundredTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var lostCategoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 800,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = lostCategoryId, CategoryName = "Food", TotalAmount = 800 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = []
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.CategoryComparativeAnalyses.Should().HaveCount(1);
        
        var lostTrend = actual.CategoryComparativeAnalyses.First();
        lostTrend.CategoryId.Should().Be(lostCategoryId);
        lostTrend.CategoryName.Should().Be("Food");
        lostTrend.SecondPeriodAmount.Should().Be(0);
        lostTrend.FirstPeriodAmount.Should().Be(800);
    }
    
    /// <summary>
    /// TC-LOST-02: Несколько исчезнувших категорий
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithMultipleLostCategoriesShouldAllHaveNegativeHundredTrend()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1000,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = Guid.NewGuid(), CategoryName = "Food", TotalAmount = 600 },
                new CategorySpendingItem { CategoryId = Guid.NewGuid(), CategoryName = "Transport", TotalAmount = 400 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = []
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.CategoryComparativeAnalyses.Should().HaveCount(2);
        
        var foodTrendLost = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Food");
        foodTrendLost.SecondPeriodAmount.Should().Be(0);
        foodTrendLost.FirstPeriodAmount.Should().Be(600);
        
        var transportTrendLost = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Transport");
        transportTrendLost.SecondPeriodAmount.Should().Be(0);
        transportTrendLost.FirstPeriodAmount.Should().Be(400);
    }
    
    /// <summary>
    /// TC-LOST-03: Комбинация - есть в обоих + исчезнувшие категории
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithMixedCategoriesIncludingLostShouldHandleCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var commonCategoryId = Guid.NewGuid();
        var lostCategoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 1200,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = commonCategoryId, CategoryName = "Food", TotalAmount = 500 },
                new CategorySpendingItem { CategoryId = lostCategoryId, CategoryName = "Transport", TotalAmount = 700 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 600,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.CategoryComparativeAnalyses.Should().HaveCount(2);
        
        var commonTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Food");
        commonTrend.FirstPeriodAmount.Should().Be(500);
        commonTrend.SecondPeriodAmount.Should().Be(600);
        
        var lostTrend = actual.CategoryComparativeAnalyses.First(x => x.CategoryName == "Transport");
        lostTrend.FirstPeriodAmount.Should().Be(700);
        lostTrend.SecondPeriodAmount.Should().Be(0);
    }
    
    /// <summary>
    /// TC-COM-04: Категории с дробными суммами - проверка округления
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithFractionalAmountsShouldRoundCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = CreateDefaultRequest(userId);
        
        var categoryId = Guid.NewGuid();
        
        var previousSpending = new CategorizedSpendingResult
        {
            TotalSpending = 123.456,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 123.456 }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 234.567,
            Categories = ImmutableArray.Create([
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
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        var trend = actual.CategoryComparativeAnalyses.First();
        trend.SecondPeriodAmount.Should().Be(234.567);
        trend.FirstPeriodAmount.Should().Be(123.456);
    }
    
    /// <summary>
    /// TC-TIME-01: TimeUnit = Day - корректное вычисление диапазонов
    /// </summary>
    [Fact]
    public async Task CategoryComparativeAnalysisWithTimeUnitDayShouldCalculateCorrectDateRanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var firstDate = new DateOnly(2020, 1, 10);
        var secondDate = new DateOnly(2020, 1, 15);
        
        var request = new CategoryComparativeAnalysisRequest
        {
            UserId = userId,
            FirstPeriod = firstDate,
            SecondPeriod = secondDate,
            TimeUnit = TimeUnit.Day,
            TimeUnitCount = 5
        };
        
        var emptyResult = new CategorizedSpendingResult
        {
            TotalSpending = 0,
            Categories = []
        };
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        
        // Настраиваем моки с проверкой дат
        _mockedTransactionRepository
            .Setup(x => x.GetCategorizedSpendingByUserIdAndDateRangeAsync(userId,
                firstDate.AddDays(-4).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                firstDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);
            
        _mockedTransactionRepository
            .Setup(x => x.GetCategorizedSpendingByUserIdAndDateRangeAsync(userId,
                secondDate.AddDays(-4).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                secondDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);
        
        // Act
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        actual.Should().NotBeNull();
    }
    #endregion
    
    #region Helper Methods
    
    private static CategoryComparativeAnalysisRequest CreateDefaultRequest(Guid userId)
    {
        return new CategoryComparativeAnalysisRequest
        {
            UserId = userId,
            FirstPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            SecondPeriod = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        };
    }
    #endregion
}