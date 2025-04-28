using FluentValidation;
using Nasurino.SmartWallet.Service.Models.Models;

namespace Nasurino.SmartWallet.Services.Validators.ModelValidators;

/// <summary>
/// Валидатор <see cref="UserModel"/>
/// </summary>
public class UserModelValidator : AbstractValidator<UserModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="UserModelValidator"/>
	/// </summary>
	public UserModelValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Id не должен быть пустым");
		RuleFor(x => x.Email)
			.NotEmpty()
			.WithMessage("Электронная почта не должна быть пустой")
			.EmailAddress()
			.WithMessage($"Строка не является адресом электронной почты");
		RuleFor(x => x.FirstName)
			.NotEmpty()
			.WithMessage("Имя не должно быть пустым");
		RuleFor(x => x.LastName)
			.NotEmpty()
			.WithMessage("Фамилия не должна быть пустой");
	}
}