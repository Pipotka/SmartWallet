using FluentValidation;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Services.Validators.CustomRules;

namespace Nasurino.SmartWallet.Services.Validators.CreateModelValidators;

/// <summary>
/// Валидатор <see cref="CreateUserModel"/>
/// </summary>
public class CreateUserModelValidator : AbstractValidator<CreateUserModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="CreateUserModelValidator"/>
	/// </summary>
	public CreateUserModelValidator(UserRepository userRepository)
	{
		RuleFor(x => x.Email)
			.NotEmpty()
			.WithMessage("Электронная почта не должна быть пустой")
			.EmailAddress()
			.WithMessage($"Строка не является адресом электронной почты")
			.MustAsync(async (email, token)
				=> await userRepository.GetUserByEmailAsync(email, token) is null)
			.WithMessage($"Пользователь с подобным адресом уже существует");
		RuleFor(x => x.Password)
			.MustBePassword();
		RuleFor(x => x.FirstName)
			.NotEmpty()
			.WithMessage("Имя не должно быть пустым");
		RuleFor(x => x.LastName)
			.NotEmpty()
			.WithMessage("Фамилия не должно быть пустым");
	}
}