using FluentValidation;
using Nasurino.SmartWallet.Service.Models.DeleteModels;

namespace Nasurino.SmartWallet.Services.Validators.DeleteModelValidators;

/// <summary>
/// Валидатор <see cref="DeleteCashVaultModel"/>
/// </summary>
public class DeleteCashVaultModelValidator : AbstractValidator<DeleteCashVaultModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="DeleteCashVaultModelValidator"/>
	/// </summary>
	public DeleteCashVaultModelValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Id не должен быть пустым");
	}
}
