namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка сервиса. Ошибка, связанная с сущностью
/// </summary>
public abstract class EntityServiceException : ServiceException
{
	protected EntityServiceException(string message, string errorCode, int statusCode)
		: base(message, errorCode, statusCode)
	{
	}
}
