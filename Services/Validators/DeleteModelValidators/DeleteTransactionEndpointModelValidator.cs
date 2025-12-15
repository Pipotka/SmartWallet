using FluentValidation;
using Nasurino.SmartWallet.Service.Models.DeleteModels;

namespace Nasurino.SmartWallet.Services.Validators.DeleteModelValidators;

/// <summary>
/// Валидатор <see cref="DeleteTransactionEndpointModel"/>
/// </summary>
public class DeleteTransactionEndpointModelValidator : AbstractValidator<DeleteTransactionEndpointModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="DeleteTransactionEndpointModelValidator"/>
	/// </summary>
	public DeleteTransactionEndpointModelValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Id не должен быть пустым");
		RuleFor(x => x.UserId)
			.NotEmpty()
			.WithMessage("Идентификатор пользователя не должен быть пустым");
	}
}
