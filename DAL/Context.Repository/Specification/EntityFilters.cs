using Nasurino.SmartWallet.Entities.Contracts;

namespace Nasurino.SmartWallet.Context.Repository.Specification;

/// <summary>
/// Спецификация фильтров сущностей
/// </summary>
public static class EntityFilters
{
	/// <summary>
	/// Фильтрует сущности, исключая мягко удалённые (для <see cref="ISmartDeletedEntity"/>)
	/// </summary>
	public static IQueryable<TEntity> NotDeleted<TEntity>(this IQueryable<TEntity> query) where TEntity : class
		=> typeof(ISmartDeletedEntity).IsAssignableFrom(typeof(TEntity))
			? query.Where(x => ((ISmartDeletedEntity)x).DeletedAt == null) 
			: query;
}
