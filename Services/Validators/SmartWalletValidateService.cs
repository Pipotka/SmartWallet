
using FluentValidation;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Services.Validators.CreateModelValidators;

namespace Nasurino.SmartWallet.Services.Validators;

/// <summary>
/// Сервис валиадции
/// </summary>
public class SmartWalletValidateService : ISmartWalletValidateService
{
	private readonly IDictionary<Type, IValidator> validators;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="SmartWalletValidateService"/>
	/// </summary>
	public SmartWalletValidateService()
	{
		validators = new Dictionary<Type, IValidator>();

		#region Регистрация валидаторов
		validators.Add(typeof(CreateUserModel), new CreateUserModelValidator());
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
