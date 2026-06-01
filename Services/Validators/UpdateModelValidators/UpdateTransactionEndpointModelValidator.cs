using FluentValidation;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Service.Models.UpdateModels;

namespace Nasurino.SmartWallet.Services.Validators.UpdateModelValidators;

/// <summary>
/// Валидатор <see cref="UpdateTransactionEndpointModel"/>
/// </summary>
public class UpdateTransactionEndpointModelValidator : AbstractValidator<UpdateTransactionEndpointModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="UpdateTransactionEndpointModelValidator"/>
	/// </summary>
	public UpdateTransactionEndpointModelValidator(ITransactionEndpointRepository transactionEndpointRepository)
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
					var model = await transactionEndpointRepository.GetByNameAndUserIdAsync(request.UserId, request.Name, token);
					return model is null || model.Id == request.Id;
				}
				return false;
			})
			.WithName(x => nameof(x.Name))
			.WithMessage($"Эндпоинт с подобным именем уже существует");
		RuleFor(x => x.Limitation)
			.Must(limitation =>
			{
				if (limitation == null)
				{
					return true;
				}
				return limitation > 0;
			})
			.WithMessage("Лимит должен быть больше нуля");
	}
}