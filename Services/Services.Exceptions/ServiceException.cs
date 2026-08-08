namespace Nasurino.SmartWallet.Services.Exceptions;

/// <summary>
/// Ошибка сервиса
/// </summary>
public abstract class ServiceException : Exception
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="ServiceException"/>
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    protected ServiceException(string message)
		: base(message)
	{

	}
}
