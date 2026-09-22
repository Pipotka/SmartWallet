namespace Nasurino.SmartWallet.Services.Exceptions;

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
    /// <param name="id">Идентификатор сущности</param>
    /// <param name="errorCode">Код ошибки</param>
    public EntityNotFoundByIdServiceException(Guid id, string errorCode)
		: base($"Сущность {typeof(TEntity).Name} с id = {id} не найдена", errorCode)
	{
	}
}
