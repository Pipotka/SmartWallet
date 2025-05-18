namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка аутентификации сервиса
/// </summary>
public class AuthorizationServiceException : ServiceException
{
	public AuthorizationServiceException(string message) 
		: base(message)
	{
	}
}
