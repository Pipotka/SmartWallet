using FluentValidation;
using Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;

namespace Nasurino.SmartWallet.Services.Validators.ModelValidators;

/// <summary>
/// Валидатор <see cref="CategoryComparativeAnalysisResult"/>
/// </summary>
public class SpendingTrendAnalysisRequestValidator : AbstractValidator<CategoryComparativeAnalysisRequest>
{
    private const int MaxDays = 18250;
    private const int MaxMonths = 600;
    private const int MaxYears = 50;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="SpendingTrendAnalysisRequestValidator"/>
    /// </summary>
    public SpendingTrendAnalysisRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Идентификатор пользователя не может быть пустым");
        
        RuleFor(x => x.TimeUnitCount)
            .GreaterThan(0)
            .WithMessage("Количество временных единиц должно быть больше 0");

        RuleFor(x => x.SecondPeriod)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime))
            .WithMessage("Дата окончания второго периода не может быть в будущем");

        RuleFor(x => x)
            .Must(ValidatePeriodsDoNotOverlap)
            .WithName(x => nameof(x.SecondPeriod))
            .WithMessage("Периоды не должны пересекаться. Конец первого периода должен быть строго меньше начала второго периода");

        RuleFor(x => x)
            .Must(ValidateTimeUnitCountLimit)
            .WithName(x => nameof(x.TimeUnitCount))
            .WithMessage($"Превышен максимальный лимит временных единиц: Day <= {MaxDays}, Month <= {MaxMonths}, Year <= {MaxYears}");
    }

    private bool ValidatePeriodsDoNotOverlap(CategoryComparativeAnalysisRequest request)
    {
        var firstPeriod = request.GetFirstDateRange();
        var secondPeriod = request.GetSecondDateRange();
        return firstPeriod.End < secondPeriod.Start;
    }

    private bool ValidateTimeUnitCountLimit(CategoryComparativeAnalysisRequest request)
    {
        return request.TimeUnit switch
        {
            TimeUnit.Day => request.TimeUnitCount <= MaxDays,
            TimeUnit.Month => request.TimeUnitCount <= MaxMonths,
            TimeUnit.Year => request.TimeUnitCount <= MaxYears,
            _ => false
        };
    }
}
