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

	protected ServiceException(string message, string errorCode)
		: base(message)
	{
		ErrorCode = errorCode;
	}
}
