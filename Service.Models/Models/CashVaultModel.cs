namespace Nasurino.SmartWallet.Service.Models.Models;

/// <summary>
/// Модель денежного хранилища
/// </summary>
public class CashVaultModel
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