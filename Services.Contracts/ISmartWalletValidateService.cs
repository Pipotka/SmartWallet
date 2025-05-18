namespace Services.Contracts
{
	/// <summary>
	/// Интерфейс валидатора моделей сервисов
	/// </summary>
	public interface ISmartWalletValidateService
	{
		/// <summary>
		/// Валидация модели
		/// </summary>
		Task ValidateAsync<TModel>(TModel model, CancellationToken token) where TModel : class;
	}
}