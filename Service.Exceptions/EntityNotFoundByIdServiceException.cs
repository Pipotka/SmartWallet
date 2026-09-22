namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка сервиса. Сущность не найдена по Id 
/// </summary>
public sealed class EntityNotFoundByIdServiceException<TEntity> : EntityNotFoundServiceException
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="EntityNotFoundByIdServiceException{TEntity}"/>
	/// </summary>
	public EntityNotFoundByIdServiceException(Guid id)
		: base($"Сущность {typeof(TEntity).Name} с id = {id} не найдена")
	{
	}

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="EntityNotFoundByIdServiceException{TEntity}"/>
	/// </summary>
	/// <param name="errorCode">Код ошибки</param>
	/// <param name="id">Идентификатор сущности</param>
	/// <param name="message">Сообщение об ошибке</param>
	public EntityNotFoundByIdServiceException(string errorCode, object id, string message)
		: base(message, errorCode)
	{
	}
}
