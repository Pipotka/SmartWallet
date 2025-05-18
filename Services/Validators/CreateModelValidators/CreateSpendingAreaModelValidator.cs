using FluentValidation;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Service.Models.CreateModels;

namespace Nasurino.SmartWallet.Services.Validators.CreateModelValidators;

/// <summary>
/// Валидатор <see cref="CreateSpendingAreaModel"/>
/// </summary>
public class CreateSpendingAreaModelValidator : AbstractValidator<CreateSpendingAreaModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="CreateSpendingAreaModelValidator"/>
	/// </summary>
	public CreateSpendingAreaModelValidator(ISpendingAreaRepository spendingAreaRepository)
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("Название не должно быть пустым");
		RuleFor(x => x.Value)
			.GreaterThanOrEqualTo(0.0)
			.WithMessage("Значение должно быть больше или равно нулю");
		RuleFor(x => x.UserId)
			.NotEmpty()
			.WithMessage("Идентификатор пользователя не должен быть пустым");
		RuleFor(x => x)
			.MustAsync(async (request, token) =>
			{
				if (!string.IsNullOrEmpty(request.Name)
				&& request.UserId != Guid.Empty)
				{
					return await spendingAreaRepository.GetByNameAndUserIdAsync(request.UserId, request.Name, token) is null;
				}
				return false;
			})
			.WithName(x => x.Name)
			.WithMessage($"Область трат с подобным именем уже существует");
	}
}