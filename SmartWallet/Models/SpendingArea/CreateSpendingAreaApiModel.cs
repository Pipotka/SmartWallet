namespace Nasurino.SmartWallet.Models.SpendingArea;

/// <summary>
/// Api модель создания области трат
/// </summary>
public class CreateSpendingAreaApiModel
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