using FluentValidation;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Services.Validators.CustomRules;

namespace Nasurino.SmartWallet.Services.Validators.ModelValidators;

/// <summary>
/// Валидатор <see cref="LogInModel"/>
/// </summary>
public class LogInModelValidator : AbstractValidator<LogInModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="LogInModelValidator"/>
	/// </summary>
	public LogInModelValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty()
			.WithMessage("Электронная почта не должна быть пустой")
			.EmailAddress()
			.WithMessage($"Строка не является адресом электронной почты");
		RuleFor(x => x.Password)
			.MustBePassword();
	}
}