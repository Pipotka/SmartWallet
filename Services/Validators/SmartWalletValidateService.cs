using FluentValidation;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.Models.FinancialAnalytics;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Nasurino.SmartWallet.Services.Validators.CreateModelValidators;
using Nasurino.SmartWallet.Services.Validators.DeleteModelValidators;
using Nasurino.SmartWallet.Services.Validators.ModelValidators;
using Nasurino.SmartWallet.Services.Validators.UpdateModelValidators;
using Services.Contracts;
using Services.Contracts.Models.Exceptions;

namespace Nasurino.SmartWallet.Services.Validators;

/// <summary>
/// Сервис валидации
/// </summary>
public sealed class SmartWalletValidateService : ISmartWalletValidateService
{
	private readonly IDictionary<Type, IValidator> _validators;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="SmartWalletValidateService"/>
	/// </summary>
	public SmartWalletValidateService(IUserRepository userRepository,
		ITransactionEndpointRepository transactionEndpointRepository)
	{
		_validators = new Dictionary<Type, IValidator>();
		#region Регистрация валидаторов
		_validators.Add(typeof(CreateUserModel), new CreateUserModelValidator(userRepository));
		_validators.Add(typeof(LogInModel), new LogInModelValidator());
		_validators.Add(typeof(UserModel), new UserModelValidator());
		_validators.Add(typeof(UpdateUserModel), new UpdateUserModelValidator());
		_validators.Add(typeof(CreateTransactionEndpointModel), new CreateTransactionEndpointValidator(transactionEndpointRepository));
		_validators.Add(typeof(CreateTransactionModel), new CreateTransactionModelValidator());
		_validators.Add(typeof(DeleteTransactionEndpointModel), new DeleteTransactionEndpointModelValidator());
		_validators.Add(typeof(DeleteTransactionModel), new DeleteTransactionModelValidator());
		_validators.Add(typeof(DeleteUserModel), new DeleteUserModelValidator());
		_validators.Add(typeof(UpdateTransactionEndpointModel), new UpdateTransactionEndpointModelValidator(transactionEndpointRepository));
		_validators.Add(typeof(CategoryComparativeAnalysisRequest), new SpendingTrendAnalysisRequestValidator());
		_validators.Add(typeof(CategorizingSpendingRequest), new CategorizingSpendingRequestValidator());
		_validators.Add(typeof(SpendingTrendLineRequest), new SpendingTrendLineRequestValidator());
        _validators.Add(typeof(ChangePasswordModel), new ChangePasswordModelValidator());
        #endregion

    }

    async Task ISmartWalletValidateService.ValidateAsync<TModel>(TModel model, CancellationToken token)
	{
		_validators.TryGetValue(typeof(TModel), out var validator);

		if (validator == null)
		{
			throw new InvalidOperationException($"Валидатор для {model.GetType().Name} не найден");
		}

		var validationResult = await validator.ValidateAsync(new ValidationContext<TModel>(model), token);
		if (!validationResult.IsValid)
		{
			throw new SmartWalletValidationException(validationResult.Errors.Select(x => new PropertyValidationError(x.PropertyName, x.ErrorMessage)).ToList());
		}
	}
}
