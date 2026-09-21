namespace Nasurino.SmartWallet.Models;

/// <summary>
/// Детали ошибки API (код и сообщение)
/// </summary>
public sealed class ApiErrorDetails
{
	/// <summary>
	/// Код ошибки
	/// </summary>
	public required string Code { get; set; }

	/// <summary>
	/// Сообщение об ошибке
	/// </summary>
	public required string Message { get; set; }
}
