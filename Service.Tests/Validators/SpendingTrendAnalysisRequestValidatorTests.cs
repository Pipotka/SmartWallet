using FluentAssertions;
using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;
using Nasurino.SmartWallet.Services.Validators.ModelValidators;
using Xunit;

namespace Nasurino.SmartWallet.Service.Tests.Validators;

/// <summary>
/// Тесты на валидатор <see cref="SpendingTrendAnalysisRequestValidator"/>
/// </summary>
public class SpendingTrendAnalysisRequestValidatorTests
{
    private readonly SpendingTrendAnalysisRequestValidator _validator;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="SpendingTrendAnalysisRequestValidatorTests"/>
    /// </summary>
    public SpendingTrendAnalysisRequestValidatorTests()
    {
        _validator = new SpendingTrendAnalysisRequestValidator();
    }

    public static TheoryData<SpendingTrendAnalysisRequest> GoodRequests => new()
    {
        new()
        {
            FirstDate = new DateOnly(2025, 1, 7),
            SecondDate = new DateOnly(2025, 1, 14),
            TimeUnit = TimeUnit.Day,
            TimeUnitCount = 7
        },
        new()
        {
            FirstDate = new DateOnly(2025, 1, 16),
            SecondDate = new DateOnly(2025, 2, 14),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 1
        }, 
        new()
        {
            FirstDate = new DateOnly(2025, 3, 16),
            SecondDate = new DateOnly(2025, 6, 14),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 3
        }, 
        new()
        {
            FirstDate = new DateOnly(2024, 1, 16),
            SecondDate = new DateOnly(2025, 2, 14),
            TimeUnit = TimeUnit.Year,
            TimeUnitCount = 1
        },
        new()
        {
            FirstDate = new DateOnly(2020, 1, 16),
            SecondDate = new DateOnly(2025, 2, 14),
            TimeUnit = TimeUnit.Year,
            TimeUnitCount = 5
        },
        new()
        {
            FirstDate = new DateOnly(2015, 1, 16),
            SecondDate = new DateOnly(2025, 2, 14),
            TimeUnit = TimeUnit.Year,
            TimeUnitCount = 5
        },
    };

    /// <summary>
    /// Валидатор должен успешно провалидировать модель
    /// </summary>
    [Theory]
    [MemberData(nameof(GoodRequests))]
    public async Task ValidatorShouldSuccessfullyValidateModel(SpendingTrendAnalysisRequest model)
    {
       // Arrange
       model.UserId = Guid.NewGuid();
       
       // Act
       var result = await _validator.ValidateAsync(model);

       // Assert
       result.IsValid.Should().BeTrue();
       result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Валидатор должен вернуть простые ошибки валидации
    /// </summary>
    [Fact]
    public async Task ValidatorShouldReturnSimpleValidationErrors()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var model = new SpendingTrendAnalysisRequest
        {
            UserId = Guid.Empty,
            FirstDate = today,
            SecondDate = today.AddMonths(1),
            TimeUnit = TimeUnit.Month,
            TimeUnitCount = 0
        };
       
        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(SpendingTrendAnalysisRequest.UserId));
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(SpendingTrendAnalysisRequest.SecondDate));
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(SpendingTrendAnalysisRequest.TimeUnitCount));
    }
    
    /// <summary>
    /// Валидатор должен вернуть ошибку пересечения периодов
    /// </summary>
    [Fact]
    public async Task ValidatorShouldReturnsErrorWhenPeriodsOverlap()
    {
        // Arrange
        var model = new SpendingTrendAnalysisRequest
        {
            UserId = Guid.NewGuid(),
            FirstDate = new(2020, 1, 10),
            SecondDate = new(2020, 1, 15),
            TimeUnit = TimeUnit.Day,
            TimeUnitCount = 10
        };
       
        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().ContainSingle(x => x.ErrorMessage.Contains("пересекать", StringComparison.OrdinalIgnoreCase));
    }
}