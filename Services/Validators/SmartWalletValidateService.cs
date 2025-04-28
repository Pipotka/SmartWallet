using FluentValidation;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Nasurino.SmartWallet.Services.Validators.CreateModelValidators;
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
	public SmartWalletValidateService(UserRepository userRepository)
	{
		validators = new Dictionary<Type, IValidator>();

		#region Регистрация валидаторов
		#region Валидаторы для моделей пользователя
		validators.Add(typeof(CreateUserModel), new CreateUserModelValidator(userRepository));
		validators.Add(typeof(LogInModel), new LogInModelValidator());
		validators.Add(typeof(UserModel), new UserModelValidator());
		validators.Add(typeof(UpdateUserModel), new UpdateUserModelValidator());
		#endregion

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
