namespace Nasurino.SmartWallet.Infrastructure;

/// <summary>
/// Информация об ошибке работы с API
/// </summary>
public class ApiExceptionDetails
{
	/// <summary>
	/// Код ошибки
	/// </summary>
	public int StatusCode { get; set; } = 500;

	/// <summary>
	/// Сообщение об ошибке
	/// </summary>
	public string Message { get; set; } = string.Empty;
}
