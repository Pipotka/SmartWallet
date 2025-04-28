using FluentValidation;
using Nasurino.SmartWallet.Service.Models.UpdateModels;

namespace Nasurino.SmartWallet.Services.Validators.UpdateModelValidators;

/// <summary>
/// Валидатор <see cref="UpdateUserModel"/>
/// </summary>
public class UpdateUserModelValidator : AbstractValidator<UpdateUserModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="UpdateUserModelValidator"/>
	/// </summary>
	public UpdateUserModelValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Id не должен быть пустым");
		RuleFor(x => x.FirstName)
			.NotEmpty()
			.WithMessage("Имя не должно быть пустым");
		RuleFor(x => x.LastName)
			.NotEmpty()
			.WithMessage("Фамилия не должна быть пустой");
	}
}