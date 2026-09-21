namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка сервиса. Сущность недоступна
/// </summary>
public class EntityAccessServiceException : EntityServiceException
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="EntityAccessServiceException"/>
	/// </summary>
	public EntityAccessServiceException(string message)
		: base(message, ErrorCodes.AccessDenied)
	{
	}
}
