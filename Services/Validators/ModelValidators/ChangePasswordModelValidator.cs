using FluentValidation;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Services.Validators.CustomRules;

namespace Nasurino.SmartWallet.Services.Validators.ModelValidators;

/// <summary>
/// Валидатор <see cref="ChangePasswordModel"/>
/// </summary>
public class ChangePasswordModelValidator : AbstractValidator<ChangePasswordModel>
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="ChangePasswordModelValidator"/>
    /// </summary>
    public ChangePasswordModelValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty()
            .WithMessage("Старый пароль не должен быть пустым")
            .MustBePassword();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Новый пароль не должен быть пустым")
            .MustBePassword();
    }
}