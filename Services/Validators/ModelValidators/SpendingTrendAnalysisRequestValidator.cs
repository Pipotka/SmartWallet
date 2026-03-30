using FluentValidation;
using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

namespace Nasurino.SmartWallet.Services.Validators.ModelValidators;

/// <summary>
/// Валидатор <see cref="SpendingTrendAnalysisResult"/>
/// </summary>
public class SpendingTrendAnalysisRequestValidator : AbstractValidator<SpendingTrendAnalysisRequest>
{
    private const int MaxDays = 36500;
    private const int MaxMonths = 1200;
    private const int MaxYears = 100;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="SpendingTrendAnalysisRequestValidator"/>
    /// </summary>
    public SpendingTrendAnalysisRequestValidator()
    {
        RuleFor(x => x.TimeUnitCount)
            .GreaterThan(0)
            .WithMessage("Количество временных единиц должно быть больше 0");

        RuleFor(x => x.FirstDate)
            .LessThanOrEqualTo(x => x.SecondDate)
            .WithMessage("Дата окончания первого периода должна быть меньше или равна дате окончания второго периода");

        RuleFor(x => x.SecondDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Дата окончания второго периода не может быть в будущем");

        RuleFor(x => x)
            .Must(ValidatePeriodsDoNotOverlap)
            .WithMessage("Периоды не должны пересекаться. Конец первого периода должен быть строго меньше начала второго периода");

        RuleFor(x => x)
            .Must(ValidateTimeUnitCountLimit)
            .WithMessage($"Превышен максимальный лимит: Day <= {MaxDays}, Month <= {MaxMonths}, Year <= {MaxYears}");
    }

    private bool ValidatePeriodsDoNotOverlap(SpendingTrendAnalysisRequest request)
    {
        var firstPeriod = request.GetFirstDateRange();
        var secondPeriod = request.GetSecondDateRange();
        return firstPeriod.End < secondPeriod.Start;
    }

    private bool ValidateTimeUnitCountLimit(SpendingTrendAnalysisRequest request)
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
