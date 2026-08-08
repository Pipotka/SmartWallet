using FluentValidation;
using Nasurino.SmartWallet.Services.Models;

namespace Nasurino.SmartWallet.Services.Validators.ModelValidators;

/// <summary>
/// Валидатор параметров запроса транзакций
/// </summary>
public class TransactionQueryValidator : AbstractValidator<TransactionQueryModel>
{
	public TransactionQueryValidator()
	{
		RuleFor(x => x.Page).GreaterThanOrEqualTo(1)
			.WithMessage("Номер страницы должен быть не менее 1");
		RuleFor(x => x.PageSize).InclusiveBetween(1, 100)
			.WithMessage("Размер страницы должен быть от 1 до 100");
	}
}
