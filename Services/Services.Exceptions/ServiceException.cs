namespace Nasurino.SmartWallet.Services.Exceptions;

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
    /// Инициализирует новый экземпляр <see cref="ServiceException"/>
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="errorCode">Код ошибки</param>
    protected ServiceException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="ServiceException"/>
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    protected ServiceException(string message)
        : this(message, string.Empty)
    {
    }
}
