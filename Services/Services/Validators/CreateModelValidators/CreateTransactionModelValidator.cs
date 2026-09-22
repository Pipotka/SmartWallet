using FluentValidation;
using Nasurino.SmartWallet.Services.Models.CreateModels;

namespace Nasurino.SmartWallet.Services.Validators.CreateModelValidators;

/// <summary>
/// Минимальный валидатор модели создания транзакции
/// </summary>
public sealed class CreateTransactionModelValidator : AbstractValidator<CreateTransactionModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="CreateTransactionModelValidator"/>
	/// </summary>
	public CreateTransactionModelValidator()
	{
		RuleFor(x => x.UserId)
			.NotEmpty();

		RuleFor(x => x.Postings)
			.NotNull()
			.NotEmpty();
	}
}
