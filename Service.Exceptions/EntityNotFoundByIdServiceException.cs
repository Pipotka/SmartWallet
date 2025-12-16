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
        : base($"Сущность {nameof(TEntity)} с id = {id} не найдена")
    {
    }
}