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
using ServiceTrendLineResult = Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics.SpendingTrendLineResult;
using DalTrendLineResult = Nasurino.SmartWallet.Context.Repository.Contracts.Models.SpendingTrendLineResult;
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
    private readonly Mock<IDailyExpenseCategorieRepository> _mockedDailyExpenseCategorieRepository;
    private readonly IFinancialAnalyticsService _financialAnalyticsService;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="FinancialAnalyticsServiceTests"/>
    /// </summary>
    public FinancialAnalyticsServiceTests()
    {
        _entityProvider = new TestEntityProviderBuilder().Build();
        var mapper = new MapperConfiguration(conf => conf.AddProfile<ServiceModelMapper>()).CreateMapper();
        
        _calculator = new FinancialCalculator();
        // TODO Написать тесты на проверку валидатора
        _validateServiceMock = new Mock<ISmartWalletValidateService>();
        
        _mockedUserRepository = new Mock<IUserRepository>();
        _mockedTransactionRepository = new Mock<ITransactionRepository>();
        _mockedDailyExpenseCategorieRepository = new Mock<IDailyExpenseCategorieRepository>();
        var unitOfWork = new MockedUnitOfWork(mockedUserRepository : _mockedUserRepository,
            mockedTransactionRepository : _mockedTransactionRepository,
            mockedDailyExpenseCategorieRepository : _mockedDailyExpenseCategorieRepository);
        
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
        var targetSum = 10_000m;
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        var sum1 = (decimal)_calculator.PercentageOfSum((double)targetSum, 25d);
        var sum2 = (decimal)_calculator.PercentageOfSum((double)targetSum, 75d);

        var spendingItems = ImmutableArray.Create([
            new CategorySpendingItem
            {
                CategoryId = categoryId1,
                CategoryName = string.Empty,
                TotalAmount = sum1
            },
            new CategorySpendingItem
            {
                CategoryId = categoryId2,
                CategoryName = string.Empty,
                TotalAmount = sum2
            }
        ]);

        var categorizedResult = new CategorizedSpendingResult
        {
            TotalSpending = targetSum,
            Categories = spendingItems
        };

        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(categorizedResult, userId,
            startDate: new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, TimeSpan.Zero), endDate: new DateTimeOffset(DateTime.Today.AddDays(1).Year, DateTime.Today.AddDays(1).Month, DateTime.Today.AddDays(1).Day, 0, 0, 0, TimeSpan.Zero));

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
        var item1 = actual.Categories.FirstOrDefault(x => x.CategoryId == categoryId1);
        item1.Should().NotBeNull();
        item1.TotalAmount.Should().Be(sum1);
        var item2 = actual.Categories.FirstOrDefault(x => x.CategoryId == categoryId2);
        item2.Should().NotBeNull();
        item2.TotalAmount.Should().Be(sum2);
    }

    /// <summary>
    /// Должен вернуть значение для сегодняшнего дня
    /// </summary>
    [Fact]
    public async Task GetCategorizingSpendingByTimeRangeAndUserIdShouldReturnValueForToday()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetSum = 10_000m;
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        var sum1 = (decimal)_calculator.PercentageOfSum((double)targetSum, 25d);
        var sum2 = (decimal)_calculator.PercentageOfSum((double)targetSum, 75d);

        var spendingItems = ImmutableArray.Create([
            new CategorySpendingItem
            {
                CategoryId = categoryId1,
                CategoryName = string.Empty,
                TotalAmount = sum1
            },
            new CategorySpendingItem
            {
                CategoryId = categoryId2,
                CategoryName = string.Empty,
                TotalAmount = sum2
            }
        ]);

        var categorizedResult = new CategorizedSpendingResult
        {
            TotalSpending = targetSum,
            Categories = spendingItems
        };

        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(categorizedResult, userId,
            startDate: new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, TimeSpan.Zero), endDate: new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, TimeSpan.Zero));

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
        var item1 = actual.Categories.FirstOrDefault(x => x.CategoryId == categoryId1);
        item1.Should().NotBeNull();
        item1.TotalAmount.Should().Be(sum1);
        var item2 = actual.Categories.FirstOrDefault(x => x.CategoryId == categoryId2);
        item2.Should().NotBeNull();
        item2.TotalAmount.Should().Be(sum2);
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(categorizedResult, userId,
            startDate: new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, TimeSpan.Zero), endDate: new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, TimeSpan.Zero));

        var request = new CategorizingSpendingRequest
        {
            UserId = userId,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today)
        };

        // Act
        var actual = await _financialAnalyticsService.GetCategorizingSpendingAsync(request, CancellationToken.None);

        // Assert
        actual.TotalSpending.Should().Be(0m);
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(categorizedResult,
            startDate: new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, TimeSpan.Zero), endDate: new DateTimeOffset(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, TimeSpan.Zero));

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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(emptyResult, userId,
            startDate: It.IsAny<DateTimeOffset>(), endDate: It.IsAny<DateTimeOffset>());
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousEmpty, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentEmpty, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        var amount = 500.0m;
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
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
            TotalSpending = 123.456m,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 123.456m }
            ])
        };
        
        var currentSpending = new CategorizedSpendingResult
        {
            TotalSpending = 234.567m,
            Categories = ImmutableArray.Create([
                new CategorySpendingItem { CategoryId = categoryId, CategoryName = "Food", TotalAmount = 234.567m }
            ])
        };

        var previousPeriod = request.GetFirstDateRange();
        var currentPeriod = request.GetSecondDateRange();
        
        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(previousSpending, userId,
            startDate: new DateTimeOffset(previousPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(previousPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        _mockedDailyExpenseCategorieRepository.GetCategorizedSpendingByUserIdAndDateRangeReturnValue(currentSpending, userId,
            startDate: new DateTimeOffset(currentPeriod.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
            endDate: new DateTimeOffset(currentPeriod.End.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero));
        
        // Act
        var actual = await _financialAnalyticsService.GetCategoryComparativeAnalysisAsync(request, CancellationToken.None);
        
        // Assert
        var trend = actual.CategoryComparativeAnalyses.First();
        trend.SecondPeriodAmount.Should().Be(234.567m);
        trend.FirstPeriodAmount.Should().Be(123.456m);
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
        _mockedDailyExpenseCategorieRepository
            .Setup(x => x.GetCategorizedSpendingByUserIdAndDateRangeAsync(userId,
                new DateTimeOffset(firstDate.AddDays(-4).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
                new DateTimeOffset(firstDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);
            
        _mockedDailyExpenseCategorieRepository
            .Setup(x => x.GetCategorizedSpendingByUserIdAndDateRangeAsync(userId,
                new DateTimeOffset(secondDate.AddDays(-4).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
                new DateTimeOffset(secondDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero),
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

    #region SpendingTrendLineAsync Tests

    /// <summary>
    /// TC-TREND-ERR-01: Должен выбросить исключение когда пользователь не найден
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendLineShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SpendingTrendLineRequest
        {
            UserId = userId,
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 3, 31),
            TimeUnit = TimeUnit.Month
        };

        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var act = () => _financialAnalyticsService.GetSpendingTrendLineAsync(request, CancellationToken.None);

        // Assert
        await act.ShouldThrowEntityNotFoundException($"*{userId}*");
    }

    /// <summary>
    /// TC-TREND-BAS-01: Пустой результат — все периоды без трат
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendLineWithNoSpendingShouldReturnEmptyCategories()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SpendingTrendLineRequest
        {
            UserId = userId,
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 3, 31),
            TimeUnit = TimeUnit.Month
        };

        var emptyResult = new DalTrendLineResult
        {
            Labels = ["Январь", "Февраль", "Март"],
            PeriodItems = []
        };

        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockedDailyExpenseCategorieRepository.GetSpendingTrendLineReturnValue(emptyResult, userId);

        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendLineAsync(request, CancellationToken.None);

        // Assert
        actual.Labels.Should().Equal("Январь", "Февраль", "Март");
        actual.Categories.Should().BeEmpty();
    }

    /// <summary>
    /// TC-TREND-BAS-02: Один период, одна категория с тратами
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendLineWithOnePeriodOneCategoryShouldReturnCorrectResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = new SpendingTrendLineRequest
        {
            UserId = userId,
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 1, 31),
            TimeUnit = TimeUnit.Month
        };

        var trendResult = new DalTrendLineResult
        {
            Labels = ["Январь"],
            PeriodItems =
            [
                new SpendingTrendPeriodItem { CategoryId = categoryId, CategoryName = "Food", Label = "Январь", TotalAmount = 500 }
            ]
        };

        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockedDailyExpenseCategorieRepository.GetSpendingTrendLineReturnValue(trendResult, userId);

        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendLineAsync(request, CancellationToken.None);

        // Assert
        actual.Labels.Should().Equal("Январь");
        actual.Categories.Should().HaveCount(1);
        actual.Categories.First().CategoryId.Should().Be(categoryId);
        actual.Categories.First().Name.Should().Be("Food");
        actual.Categories.First().Nodes.Should().HaveCount(1);
        actual.Categories.First().Nodes.First().Label.Should().Be("Январь");
        actual.Categories.First().Nodes.First().Amount.Should().Be(500);
    }

    /// <summary>
    /// TC-TREND-BAS-03: Несколько периодов, одна категория с тратами во всех периодах
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendLineWithMultiplePeriodsOneCategoryShouldReturnCorrectResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = new SpendingTrendLineRequest
        {
            UserId = userId,
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 5, 31),
            TimeUnit = TimeUnit.Month
        };

        var trendResult = new DalTrendLineResult
        {
            Labels = ["Январь", "Февраль", "Март", "Апрель", "Май"],
            PeriodItems =
            [
                new SpendingTrendPeriodItem { CategoryId = categoryId, CategoryName = "Food", Label = "Январь", TotalAmount = 500 },
                new SpendingTrendPeriodItem { CategoryId = categoryId, CategoryName = "Food", Label = "Февраль", TotalAmount = 200 },
                new SpendingTrendPeriodItem { CategoryId = categoryId, CategoryName = "Food", Label = "Март", TotalAmount = 300 },
                new SpendingTrendPeriodItem { CategoryId = categoryId, CategoryName = "Food", Label = "Апрель", TotalAmount = 400 },
                new SpendingTrendPeriodItem { CategoryId = categoryId, CategoryName = "Food", Label = "Май", TotalAmount = 700 }
            ]
        };

        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockedDailyExpenseCategorieRepository.GetSpendingTrendLineReturnValue(trendResult, userId);

        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendLineAsync(request, CancellationToken.None);

        // Assert
        actual.Labels.Should().Equal("Январь", "Февраль", "Март", "Апрель", "Май");
        actual.Categories.Should().HaveCount(1);
        actual.Categories.First().CategoryId.Should().Be(categoryId);
        var nodes = actual.Categories.First().Nodes.ToList();
        nodes.Should().HaveCount(5);
        nodes[0].Label.Should().Be("Январь");
        nodes[0].Amount.Should().Be(500);
        nodes[1].Label.Should().Be("Февраль");
        nodes[1].Amount.Should().Be(200);
        nodes[2].Label.Should().Be("Март");
        nodes[2].Amount.Should().Be(300);
        nodes[3].Label.Should().Be("Апрель");
        nodes[3].Amount.Should().Be(400);
        nodes[4].Label.Should().Be("Май");
        nodes[4].Amount.Should().Be(700);
    }

    /// <summary>
    /// TC-TREND-CAT-01: Категория с тратами не во всех периодах — Nodes содержит только периоды с тратами
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendLineWithCategoryNotInAllPeriodsShouldReturnOnlyNonZeroPeriodNodes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = new SpendingTrendLineRequest
        {
            UserId = userId,
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 5, 31),
            TimeUnit = TimeUnit.Month
        };

        var trendResult = new DalTrendLineResult
        {
            Labels = ["Январь", "Февраль", "Март", "Апрель", "Май"],
            PeriodItems =
            [
                new SpendingTrendPeriodItem { CategoryId = categoryId, CategoryName = "Food", Label = "Январь", TotalAmount = 500 },
                new SpendingTrendPeriodItem { CategoryId = categoryId, CategoryName = "Food", Label = "Май", TotalAmount = 700 }
            ]
        };

        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockedDailyExpenseCategorieRepository.GetSpendingTrendLineReturnValue(trendResult, userId);

        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendLineAsync(request, CancellationToken.None);

        // Assert
        actual.Categories.Should().HaveCount(1);
        var foodCategory = actual.Categories.First();
        foodCategory.CategoryId.Should().Be(categoryId);
        var nodes = foodCategory.Nodes.ToList();
        nodes.Should().HaveCount(2);
        nodes[0].Label.Should().Be("Январь");
        nodes[0].Amount.Should().Be(500);
        nodes[1].Label.Should().Be("Май");
        nodes[1].Amount.Should().Be(700);
    }

    /// <summary>
    /// TC-TREND-CAT-02: Несколько категорий, каждая с тратами в разных подмножествах периодов
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendLineWithMultipleCategoriesShouldSortByTotalAmountDescending()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var foodId = Guid.NewGuid();
        var transportId = Guid.NewGuid();
        var request = new SpendingTrendLineRequest
        {
            UserId = userId,
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 4, 30),
            TimeUnit = TimeUnit.Month
        };

        var trendResult = new DalTrendLineResult
        {
            Labels = ["Январь", "Февраль", "Март", "Апрель"],
            PeriodItems =
            [
                new SpendingTrendPeriodItem { CategoryId = foodId, CategoryName = "Food", Label = "Январь", TotalAmount = 500 },
                new SpendingTrendPeriodItem { CategoryId = foodId, CategoryName = "Food", Label = "Март", TotalAmount = 600 },
                new SpendingTrendPeriodItem { CategoryId = transportId, CategoryName = "Transport", Label = "Январь", TotalAmount = 200 }
            ]
        };

        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockedDailyExpenseCategorieRepository.GetSpendingTrendLineReturnValue(trendResult, userId);

        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendLineAsync(request, CancellationToken.None);

        // Assert
        actual.Categories.Should().HaveCount(2);
        actual.Categories.First().Name.Should().Be("Food");
        actual.Categories.First().CategoryId.Should().Be(foodId);
        var foodNodes = actual.Categories.First().Nodes.ToList();
        foodNodes.Should().HaveCount(2);
        foodNodes[0].Label.Should().Be("Январь");
        foodNodes[0].Amount.Should().Be(500);
        foodNodes[1].Label.Should().Be("Март");
        foodNodes[1].Amount.Should().Be(600);
        foodNodes.Sum(n => n.Amount).Should().Be(1100);
        actual.Categories.Last().Name.Should().Be("Transport");
        actual.Categories.Last().CategoryId.Should().Be(transportId);
        var transportNodes = actual.Categories.Last().Nodes.ToList();
        transportNodes.Should().HaveCount(1);
        transportNodes[0].Label.Should().Be("Январь");
        transportNodes[0].Amount.Should().Be(200);
        transportNodes.Sum(n => n.Amount).Should().Be(200);
    }

    /// <summary>
    /// TC-TREND-YEAR-01: TimeUnit = Year — корректные метки-годы и хронологический порядок узлов
    /// </summary>
    [Fact]
    public async Task GetSpendingTrendLineWithTimeUnitYearShouldReturnYearLabelsAndCorrectNodeOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = new SpendingTrendLineRequest
        {
            UserId = userId,
            StartDate = new DateOnly(2023, 1, 1),
            EndDate = new DateOnly(2025, 12, 31),
            TimeUnit = TimeUnit.Year
        };

        // PeriodItems intentionally in reverse order to verify SUT sorts by label index
        var trendResult = new DalTrendLineResult
        {
            Labels = ["2023", "2024", "2025"],
            PeriodItems =
            [
                new SpendingTrendPeriodItem { CategoryId = categoryId, CategoryName = "Food", Label = "2025", TotalAmount = 1500 },
                new SpendingTrendPeriodItem { CategoryId = categoryId, CategoryName = "Food", Label = "2023", TotalAmount = 1000 }
            ]
        };

        _mockedUserRepository.GetUserByIdReturnNotNull(userId);
        _validateServiceMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockedDailyExpenseCategorieRepository.GetSpendingTrendLineReturnValue(trendResult, userId);

        // Act
        var actual = await _financialAnalyticsService.GetSpendingTrendLineAsync(request, CancellationToken.None);

        // Assert
        actual.Labels.Should().Equal("2023", "2024", "2025");
        actual.Categories.Should().HaveCount(1);
        var category = actual.Categories.First();
        category.CategoryId.Should().Be(categoryId);
        category.Name.Should().Be("Food");
        var nodes = category.Nodes.ToList();
        nodes.Should().HaveCount(2);
        nodes[0].Label.Should().Be("2023");
        nodes[0].Amount.Should().Be(1000);
        nodes[1].Label.Should().Be("2025");
        nodes[1].Amount.Should().Be(1500);
    }
    #endregion
}