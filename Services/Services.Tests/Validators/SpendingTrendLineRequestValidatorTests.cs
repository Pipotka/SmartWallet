using FluentAssertions;
using Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;
using Nasurino.SmartWallet.Services.Validators.ModelValidators;
using Xunit;

namespace Nasurino.SmartWallet.Services.Tests.Validators;

/// <summary>
/// Тесты на валидатор <see cref="SpendingTrendLineRequestValidator"/>
/// </summary>
public class SpendingTrendLineRequestValidatorTests
{
    private readonly SpendingTrendLineRequestValidator _validator;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="SpendingTrendLineRequestValidatorTests"/>
    /// </summary>
    public SpendingTrendLineRequestValidatorTests()
    {
        _validator = new SpendingTrendLineRequestValidator();
    }

    public static TheoryData<SpendingTrendLineRequest> GoodRequests => new()
    {
        new()
        {
            UserId = Guid.NewGuid(),
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 3, 31),
            TimeUnit = TimeUnit.Month
        },
        new()
        {
            UserId = Guid.NewGuid(),
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 12, 31),
            TimeUnit = TimeUnit.Month
        },
        new()
        {
            UserId = Guid.NewGuid(),
            StartDate = new DateOnly(2020, 1, 1),
            EndDate = new DateOnly(2025, 12, 31),
            TimeUnit = TimeUnit.Year
        },
        new()
        {
            UserId = Guid.NewGuid(),
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 1, 31),
            TimeUnit = TimeUnit.Day
        }
    };

    public static TheoryData<TimeUnit, DateOnly, DateOnly> NodeCountExceedsLimitCases => new()
    {
        { TimeUnit.Day, new DateOnly(2024, 1, 1), new DateOnly(2025, 1, 1) },
        { TimeUnit.Month, new DateOnly(2015, 1, 1), new DateOnly(2025, 1, 31) },
        { TimeUnit.Year, new DateOnly(1975, 1, 1), new DateOnly(2025, 12, 31) }
    };

    public static TheoryData<TimeUnit, DateOnly, DateOnly> NodeCountBoundaryCases => new()
    {
        { TimeUnit.Day, new DateOnly(2023, 1, 1), new DateOnly(2023, 12, 31) },
        { TimeUnit.Month, new DateOnly(2015, 1, 1), new DateOnly(2024, 12, 31) },
        { TimeUnit.Year, new DateOnly(1975, 1, 1), new DateOnly(2024, 12, 31) }
    };

    /// <summary>
    /// Валидатор должен успешно провалидировать модель
    /// </summary>
    [Theory]
    [MemberData(nameof(GoodRequests))]
    public async Task ValidatorShouldSuccessfullyValidateModel(SpendingTrendLineRequest model)
    {
        // Arrange & Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Валидатор должен вернуть ошибку при пустом UserId
    /// </summary>
    [Fact]
    public async Task ValidatorShouldReturnErrorWhenUserIdIsEmpty()
    {
        // Arrange
        var model = new SpendingTrendLineRequest
        {
            UserId = Guid.Empty,
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 3, 31),
            TimeUnit = TimeUnit.Month
        };

        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(SpendingTrendLineRequest.UserId));
    }

    /// <summary>
    /// Валидатор должен вернуть ошибку при StartDate в будущем
    /// </summary>
    [Fact]
    public async Task ValidatorShouldReturnErrorWhenStartDateIsInFuture()
    {
        // Arrange
        var model = new SpendingTrendLineRequest
        {
            UserId = Guid.NewGuid(),
            StartDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime).AddDays(1),
            EndDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime).AddDays(10),
            TimeUnit = TimeUnit.Day
        };

        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(SpendingTrendLineRequest.StartDate));
    }

    /// <summary>
    /// Валидатор должен вернуть ошибку при EndDate в будущем
    /// </summary>
    [Fact]
    public async Task ValidatorShouldReturnErrorWhenEndDateIsInFuture()
    {
        // Arrange
        var model = new SpendingTrendLineRequest
        {
            UserId = Guid.NewGuid(),
            StartDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime).AddMonths(-2),
            EndDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime).AddDays(1),
            TimeUnit = TimeUnit.Month
        };

        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(SpendingTrendLineRequest.EndDate));
    }

    /// <summary>
    /// Валидатор должен вернуть ошибку при StartDate >= EndDate
    /// </summary>
    [Fact]
    public async Task ValidatorShouldReturnErrorWhenStartDateGreaterThanOrEqualEndDate()
    {
        // Arrange
        var model = new SpendingTrendLineRequest
        {
            UserId = Guid.NewGuid(),
            StartDate = new DateOnly(2025, 3, 31),
            EndDate = new DateOnly(2025, 1, 1),
            TimeUnit = TimeUnit.Month
        };

        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("строго меньше", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Валидатор должен вернуть ошибку при StartDate == EndDate
    /// </summary>
    [Fact]
    public async Task ValidatorShouldReturnErrorWhenStartDateEqualsEndDate()
    {
        // Arrange
        var model = new SpendingTrendLineRequest
        {
            UserId = Guid.NewGuid(),
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 1, 1),
            TimeUnit = TimeUnit.Month
        };

        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("строго меньше", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Валидатор должен вернуть ошибку при превышении максимального количества узлов
    /// </summary>
    [Theory]
    [MemberData(nameof(NodeCountExceedsLimitCases))]
    public async Task ValidatorShouldReturnErrorWhenNodeCountExceedsMaxLimit(
        TimeUnit timeUnit, DateOnly startDate, DateOnly endDate)
    {
        // Arrange
        var model = new SpendingTrendLineRequest
        {
            UserId = Guid.NewGuid(),
            StartDate = startDate,
            EndDate = endDate,
            TimeUnit = timeUnit
        };

        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.ErrorMessage.Contains("лимит", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Валидатор должен принять корректные значения количества узлов на границах
    /// </summary>
    [Theory]
    [MemberData(nameof(NodeCountBoundaryCases))]
    public async Task ValidatorShouldAcceptBoundaryNodeCountValues(
        TimeUnit timeUnit, DateOnly startDate, DateOnly endDate)
    {
        // Arrange
        var model = new SpendingTrendLineRequest
        {
            UserId = Guid.NewGuid(),
            StartDate = startDate,
            EndDate = endDate,
            TimeUnit = timeUnit
        };

        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
