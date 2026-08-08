using Nasurino.SmartWallet.Services.Contracts.Models.Exceptions;

namespace Nasurino.SmartWallet.Services.Exceptions;

/// <summary>
/// Ошибка валидации
/// </summary>
public class SmartWalletValidationException : EntityServiceException
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="SmartWalletValidationException"/>
	/// </summary>
	/// <param name="validationResults">Результаты валидации</param>
	public SmartWalletValidationException(ICollection<PropertyValidationError> validationResults)
		: base(string.Join(';', validationResults.Select(x => $"{x.PropertyName} - {x.ErrorMessage}")))
	{

	}
	
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="SmartWalletValidationException"/>
	/// </summary>
	/// <param name="validationResult">Результат валидации</param>
	public SmartWalletValidationException(PropertyValidationError validationResult)
		: base($"{validationResult.PropertyName} - {validationResult.ErrorMessage}")
	{

	}
}
