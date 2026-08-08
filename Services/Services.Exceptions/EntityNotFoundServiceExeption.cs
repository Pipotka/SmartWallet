namespace Nasurino.SmartWallet.Services.Exceptions;

/// <summary>
/// Ошибка сервиса. Сущность не найдена
/// </summary>
public class EntityNotFoundServiceException : EntityServiceException
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="EntityNotFoundServiceException"/>
	/// </summary>
	public EntityNotFoundServiceException(string message) 
		: base(message)
	{
	}
}