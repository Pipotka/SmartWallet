using FluentValidation;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Service.Models.CreateModels;

namespace Nasurino.SmartWallet.Services.Validators.CreateModelValidators;

/// <summary>
/// Валидатор <see cref="CreateTransactionEndpointModel"/>
/// </summary>
public class CreateTransactionEndpointValidator : AbstractValidator<CreateTransactionEndpointModel>
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="CreateTransactionEndpointValidator"/>
	/// </summary>
	public CreateTransactionEndpointValidator(ITransactionEndpointRepository transactionEndpointRepository)
	{
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
				&& request.UserId != Guid.Empty)
				{
					return await transactionEndpointRepository.GetByNameAndUserIdAsync(request.UserId, request.Name, token) is null;
				}
				return false;
			})
			.WithName(x => x.Name)
			.WithMessage($"Денежное хранилище с подобным именем уже существует");
	}
}
