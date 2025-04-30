namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка сервиса. Сущность недоступна
/// </summary>
public class EntityAccessServiceException : EntityServiceException
{
	public EntityAccessServiceException(string message) 
		: base(message)
	{
	}
}