using FluentValidation;
using Nasurino.SmartWallet.Service.Models.DeleteModels;

namespace Nasurino.SmartWallet.Services.Validators.DeleteModelValidators;

/// <summary>
/// Валидатор <see cref="DeleteSpendingAreaModel"/>
/// </summary>
public class DeleteSpendingAreaModelValidator : AbstractValidator<DeleteSpendingAreaModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="DeleteSpendingAreaModelValidator"/>
	/// </summary>
	public DeleteSpendingAreaModelValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Id не должен быть пустым");
	}
}
