using Microsoft.AspNetCore.Http;

namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка аутентификации сервиса
/// </summary>
public class AuthenticationServiceException : ServiceException
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="AuthenticationServiceException"/>
	/// </summary>
	public AuthenticationServiceException()
		: base("Ошибка аутентификации.", ErrorCodes.Unauthorized, StatusCodes.Status401Unauthorized)
	{
	}

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="AuthenticationServiceException"/>
	/// </summary>
	public AuthenticationServiceException(string message)
		: base(message, ErrorCodes.Unauthorized, StatusCodes.Status401Unauthorized)
	{
	}
}
