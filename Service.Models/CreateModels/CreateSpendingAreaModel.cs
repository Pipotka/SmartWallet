namespace Nasurino.SmartWallet.Service.Models.CreateModels;

/// <summary>
/// Модель создания области трат
/// </summary>
public class CreateSpendingAreaModel
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