using FluentValidation;
using Nasurino.SmartWallet.Service.Models.Models;

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
			.Length(8, 33)
			.WithMessage("Пароль должен быть больше 8 и меньше 34 символов")
			.Matches(@"^(?=.*[@#$!^%&*()\-_+=]).+$")
			.WithMessage("Пароль должен содержать спецсимволы")
			.Matches(@"^(?=.*\p{Lu}).+$")
			.WithMessage("Пароль должен содержать заглавную букву");
	}
}
