namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка сервиса. Сущность не найдена
/// </summary>
public class EntityNotFoundServiceException : EntityServiceException
{
	public EntityNotFoundServiceException(string message) 
		: base(message)
	{
	}
}