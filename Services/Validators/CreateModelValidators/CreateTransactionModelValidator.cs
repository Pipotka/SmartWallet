using FluentValidation;
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
		RuleFor(x => x.ToSpendingAreaId)
			.NotEmpty()
			.WithMessage("Назначение не должно быть пустым");
		RuleFor(x => x.FromCashVaultId)
			.NotEmpty()
			.WithMessage("источник не должен быть пустым");
		RuleFor(x => x.Value)
			.GreaterThanOrEqualTo(0.0)
			.WithMessage("Значение должно быть больше или равно нулю");
	}
}
