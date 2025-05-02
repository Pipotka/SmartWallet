using FluentValidation;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Services.Validators.CustomRules;

namespace Nasurino.SmartWallet.Services.Validators.DeleteModelValidators;

/// <summary>
/// Валидатор <see cref="DeleteUserModel"/>
/// </summary>
public class DeleteUserModelValidator : AbstractValidator<DeleteUserModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="DeleteUserModelValidator"/>
	/// </summary>
	public DeleteUserModelValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Id не должен быть пустым");
		RuleFor(x => x.Password)
			.MustBePassword();
	}
}