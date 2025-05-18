using FluentValidation;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Service.Models.CreateModels;

namespace Nasurino.SmartWallet.Services.Validators.CreateModelValidators;

/// <summary>
/// Валидатор <see cref="CreateCashVaultModel"/>
/// </summary>
public class CreateCashVaultModelValidator : AbstractValidator<CreateCashVaultModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="CreateCashVaultModelValidator"/>
	/// </summary>
	public CreateCashVaultModelValidator(ICashVaultRepository cashVaultRepository)
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
					return await cashVaultRepository.GetByNameAndUserIdAsync(request.UserId, request.Name, token) is null;
				}
				return false;
			})
			.WithName(x => x.Name)
			.WithMessage($"Денежное хранилище с подобным именем уже существует");
	}
}
