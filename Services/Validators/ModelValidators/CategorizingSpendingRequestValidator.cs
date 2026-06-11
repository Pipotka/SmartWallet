using FluentValidation;
using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;

namespace Nasurino.SmartWallet.Services.Validators.ModelValidators;

/// <summary>
/// Валидатор <see cref="CategorizingSpendingRequest"/>
/// </summary>
public class CategorizingSpendingRequestValidator : AbstractValidator<CategorizingSpendingRequest>
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="CategorizingSpendingRequestValidator"/>
    /// </summary>
    public CategorizingSpendingRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Идентификатор пользователя не может быть пустым");

        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime);
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(today)
            .WithMessage("Начало временного диапазона не может быть в будущем");

        var nextMonth = today.AddMonths(1);

        RuleFor(x => x.EndDate)
            .LessThanOrEqualTo(new DateOnly(nextMonth.Year, nextMonth.Month, 1))
            .WithMessage("Временной диапазон не может затрагивать следующий месяц");

        RuleFor(x => x)
            .Must(x => x.StartDate <= x.EndDate)
            .WithName(x => nameof(x.StartDate))
            .WithMessage("Дата начала должна быть меньше или ровна дате конца временного диапазона");
    }
}