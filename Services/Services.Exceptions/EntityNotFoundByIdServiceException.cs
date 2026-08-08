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
}