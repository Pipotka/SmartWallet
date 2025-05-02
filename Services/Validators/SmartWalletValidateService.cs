using FluentValidation;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Nasurino.SmartWallet.Services.Validators.CreateModelValidators;
using Nasurino.SmartWallet.Services.Validators.DeleteModelValidators;
using Nasurino.SmartWallet.Services.Validators.ModelValidators;
using Nasurino.SmartWallet.Services.Validators.UpdateModelValidators;

namespace Nasurino.SmartWallet.Services.Validators;

/// <summary>
/// Сервис валиадции
/// </summary>
public class SmartWalletValidateService
{
	private readonly IDictionary<Type, IValidator> validators;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="SmartWalletValidateService"/>
	/// </summary>
	public SmartWalletValidateService(UserRepository userRepository,
		CashVaultRepository cashVaultRepository,
		SpendingAreaRepository spendingAreaRepository)
	{
		validators = new Dictionary<Type, IValidator>();
		#region Регистрация валидаторов
		validators.Add(typeof(CreateUserModel), new CreateUserModelValidator(userRepository));
		validators.Add(typeof(LogInModel), new LogInModelValidator());
		validators.Add(typeof(UserModel), new UserModelValidator());
		validators.Add(typeof(UpdateUserModel), new UpdateUserModelValidator());
		validators.Add(typeof(CreateCashVaultModel), new CreateCashVaultModelValidator(cashVaultRepository));
		validators.Add(typeof(CreateSpendingAreaModel), new CreateSpendingAreaModelValidator(spendingAreaRepository));
		validators.Add(typeof(CreateTransactionModel), new CreateTransactionModelValidator());
		validators.Add(typeof(DeleteCashVaultModel), new DeleteCashVaultModelValidator());
		validators.Add(typeof(DeleteSpendingAreaModel), new DeleteSpendingAreaModelValidator());
		validators.Add(typeof(DeleteTransactionModel), new DeleteTransactionModelValidator());
		validators.Add(typeof(DeleteUserModel), new DeleteUserModelValidator());
		validators.Add(typeof(UpdateCashVaultModel), new UpdateCashVaultModelValidator(cashVaultRepository));
		validators.Add(typeof(UpdateSpendingAreaModel), new UpdateSpendingAreaModelValidator(spendingAreaRepository));
		#endregion

	}

	/// <summary>
	/// Валидация модели
	/// </summary>
	public async Task ValidateAsync<TModel>(TModel model, CancellationToken token) where TModel : class
	{
		validators.TryGetValue(typeof(TModel), out var validator);

		if (validator == null)
		{
			throw new InvalidOperationException($"Валидатор для {model.GetType().Name} не найден");
		}

		var validationResult = await validator.ValidateAsync(new ValidationContext<TModel>(model), token);
		if (!validationResult.IsValid)
		{
			throw new SmartWalletValidationException(string.Join(';', validationResult.Errors.Select(x => $"{x.PropertyName} - {x.ErrorMessage}")));
		}
	}
}
