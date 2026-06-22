using FluentValidation;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Models.CreateModels;

namespace Nasurino.SmartWallet.Services.Validators.CreateModelValidators;

/// <summary>
/// Валидатор <see cref="CreateTransactionModel"/>
/// </summary>
public class CreateTransactionModelValidator : AbstractValidator<CreateTransactionModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="CreateTransactionModelValidator"/>
	/// </summary>
	public CreateTransactionModelValidator()
	{
		RuleFor(x => x)
			.Must(x => x.SourceAccountId.HasValue || x.DestinationAccountId.HasValue)
			.WithMessage($"По крайней мере одно из свойств ({nameof(CreateTransactionModel.SourceAccountId)} или {nameof(CreateTransactionModel.DestinationAccountId)}) должно иметь значение.");
		RuleFor(x => x.DestinationAccountId)
			.NotEqual(x => x.SourceAccountId)
			.WithMessage($"Нельзя создать {nameof(Transaction)}, где {nameof(Transaction.SourceAccountId)} и {nameof(Transaction.DestinationAccountId)} имеют одинаковые значения");
		RuleFor(x => x.Amount)
			.GreaterThan(0.0)
			.WithMessage("Значение должно быть больше нуля");
	}
}
