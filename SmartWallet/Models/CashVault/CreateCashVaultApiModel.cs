namespace Nasurino.SmartWallet.Models.CashVault;

/// <summary>
/// Модель Api для создания денежного хранилища
/// </summary>
public class CreateCashVaultApiModel
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