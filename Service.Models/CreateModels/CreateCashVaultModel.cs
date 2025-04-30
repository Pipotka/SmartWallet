namespace Nasurino.SmartWallet.Service.Models.CreateModels;

/// <summary>
/// Модель создания денежного хранилища
/// </summary>
public class CreateCashVaultModel
{
	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Значение
	/// </summary>
	public double Value { get; set; } = 0.0;
}