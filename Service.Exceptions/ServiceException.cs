namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка сервиса
/// </summary>
public abstract class ServiceException : Exception
{
	/// <summary>
	/// Код ошибки
	/// </summary>
	public string ErrorCode { get; }

	/// <summary>
	/// HTTP-статус ответа
	/// </summary>
	public int StatusCode { get; }

	protected ServiceException(string message, string errorCode, int statusCode)
		: base(message)
	{
		ErrorCode = errorCode;
		StatusCode = statusCode;
	}
}
