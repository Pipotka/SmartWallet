using Nasurino.SmartWallet.Entities.Contracts;

namespace Nasurino.SmartWallet.Context.Repository.Contracts.Specification;

/// <summary>
/// Спецификация фильтров сущностей
/// </summary>
public static class EntityFilters
{
	/// <summary>
	/// Фильтрует сущности, исключая мягко удалённые (для <see cref="ISmartDeletedEntity"/>)
	/// </summary>
	public static IQueryable<TEntity> NotDeleted<TEntity>(this IQueryable<TEntity> query) where TEntity : ISmartDeletedEntity
		=>query.Where(x => x.DeletedAt == null);
}
