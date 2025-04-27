namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка аутентификации сервиса
/// </summary>
public class AuthenticationServiceException : ServiceException
{
	public AuthenticationServiceException(string message) 
		: base(message)
	{
	}
}
