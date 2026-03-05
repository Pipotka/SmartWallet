namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка авторизации сервиса
/// </summary>
public class AuthorizationServiceException : ServiceException
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="AuthorizationServiceException"/>
	/// </summary>
	public AuthorizationServiceException(string message) 
		: base(message)
	{
	}
}
