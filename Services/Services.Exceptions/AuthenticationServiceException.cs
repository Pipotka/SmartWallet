namespace Nasurino.SmartWallet.Services.Exceptions;

/// <summary>
/// Ошибка аутентификации сервиса
/// </summary>
public class AuthenticationServiceException : ServiceException
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="AuthenticationServiceException"/>
    /// </summary>
    public AuthenticationServiceException()
        : base("Ошибка аутентификации.")
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="AuthenticationServiceException"/>
    /// </summary>
    public AuthenticationServiceException(string message) 
        : base(message)
    {
    }
}