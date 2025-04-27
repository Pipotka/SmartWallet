using FluentValidation;
using Nasurino.SmartWallet.Service.Models.CreateModels;

namespace Nasurino.SmartWallet.Services.Validators.CreateModelValidators;

/// <summary>
/// Валидатор <see cref="CreateUserModel"/>
/// </summary>
public class CreateUserModelValidator : AbstractValidator<CreateUserModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="CreateUserModelValidator"/>
	/// </summary>
	public CreateUserModelValidator()
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
		RuleFor(x => x.FirstName)
			.NotEmpty()
			.WithMessage("Имя не должно быть пустым");
		RuleFor(x => x.LastName)
			.NotEmpty()
			.WithMessage("Фамилия не должно быть пустым");
	}
}