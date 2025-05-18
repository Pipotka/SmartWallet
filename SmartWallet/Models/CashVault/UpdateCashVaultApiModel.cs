namespace Nasurino.SmartWallet.Models.CashVault;

/// <summary>
/// Модель Api для обновления денежного хранилища
/// </summary>
public class UpdateCashVaultApiModel
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Значение
	/// </summary>
	public double Value { get; set; } = 0.0;
}