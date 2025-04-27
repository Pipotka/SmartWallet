namespace Nasurino.SmartWallet.Services.Validators;

/// <summary>
/// Интерфейс валидации SmartWallet моделей
/// </summary>
public interface ISmartWalletValidateService
{
	/// <summary>
	/// Валидация модели
	/// </summary>
	Task ValidateAsync<TModel>(TModel model, CancellationToken token) where TModel : class;
}
