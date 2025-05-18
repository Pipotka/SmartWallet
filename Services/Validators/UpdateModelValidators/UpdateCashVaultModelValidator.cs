using FluentValidation;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Service.Models.UpdateModels;

namespace Nasurino.SmartWallet.Services.Validators.UpdateModelValidators;

/// <summary>
/// Валидатор <see cref="UpdateCashVaultModel"/>
/// </summary>
public class UpdateCashVaultModelValidator : AbstractValidator<UpdateCashVaultModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="UpdateCashVaultModelValidator"/>
	/// </summary>
	public UpdateCashVaultModelValidator(ICashVaultRepository cashVaultRepository)
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Id не должен быть пустым");
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("Название не должно быть пустым");
		RuleFor(x => x.UserId)
			.NotEmpty()
			.WithMessage("Идентификатор пользователя не должен быть пустым");
		RuleFor(x => x)
			.MustAsync(async (request, token) =>
			{
				if (!string.IsNullOrEmpty(request.Name)
				&& request.UserId != Guid.Empty
				&& request.Id != Guid.Empty)
				{
					var model = await cashVaultRepository.GetByNameAndUserIdAsync(request.UserId, request.Name, token);
					return model is null || model.Id == request.Id;
				}
				return false;
			})
			.WithName(x => x.Name)
			.WithMessage($"Денежное хранилище с подобным именем уже существует");
	}
}