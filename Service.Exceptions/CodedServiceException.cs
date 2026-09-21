namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Кодированное исключение сервиса с явным кодом ошибки
/// </summary>
public sealed class CodedServiceException : ServiceException
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="CodedServiceException"/>
	/// </summary>
	public CodedServiceException(string errorCode, string message)
		: base(message, errorCode)
	{
	}
}
