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
        : base("Аутентификация провалилась. Неверный логин или пароль.")
    {
    }
}