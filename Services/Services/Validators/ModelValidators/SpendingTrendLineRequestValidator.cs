using FluentValidation;
using Nasurino.SmartWallet.Services.Models.Models.FinancialAnalytics;

namespace Nasurino.SmartWallet.Services.Validators.ModelValidators;

/// <summary>
/// Валидатор <see cref="SpendingTrendLineRequest"/>
/// </summary>
public class SpendingTrendLineRequestValidator : AbstractValidator<SpendingTrendLineRequest>
{
    private const int MaxDays = 365;
    private const int MaxMonths = 120;
    private const int MaxYears = 50;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="SpendingTrendLineRequestValidator"/>
    /// </summary>
    public SpendingTrendLineRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Идентификатор пользователя не может быть пустым");

        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime);
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(today)
            .WithMessage("Дата начала не может быть в будущем");

        RuleFor(x => x.EndDate)
            .LessThanOrEqualTo(today)
            .WithMessage("Дата окончания не может быть в будущем");

        RuleFor(x => x)
            .Must(x => x.StartDate < x.EndDate)
            .WithName(x => nameof(x.StartDate))
            .WithMessage("Дата начала должна быть строго меньше даты окончания");

        RuleFor(x => x)
            .Must(ValidateNodeCountLimit)
            .WithName(x => nameof(x.TimeUnit))
            .WithMessage($"Превышен максимальный лимит узлов: Day <= {MaxDays}, Month <= {MaxMonths}, Year <= {MaxYears}");
    }

    private bool ValidateNodeCountLimit(SpendingTrendLineRequest request)
    {
        return request.TimeUnit switch
        {
            TimeUnit.Day => (request.EndDate.DayNumber - request.StartDate.DayNumber) + 1 <= MaxDays,
            TimeUnit.Month => ((request.EndDate.Year - request.StartDate.Year) * 12 + request.EndDate.Month - request.StartDate.Month) + 1 <= MaxMonths,
            TimeUnit.Year => (request.EndDate.Year - request.StartDate.Year) + 1 <= MaxYears,
            _ => false
        };
    }
}