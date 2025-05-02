using FluentValidation;
using Nasurino.SmartWallet.Service.Models.DeleteModels;

namespace Nasurino.SmartWallet.Services.Validators.DeleteModelValidators;

/// <summary>
/// Валидатор <see cref="DeleteTransactionModel"/>
/// </summary>
public class DeleteTransactionModelValidator : AbstractValidator<DeleteTransactionModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="DeleteTransactionModelValidator"/>
	/// </summary>
	public DeleteTransactionModelValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Id не должен быть пустым");
	}
}