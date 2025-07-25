using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;

namespace Nasurino.SmartWallet.Context.Repository.Extensions;

/// <summary>
/// Методы расширения для <see cref="IQueryable"/>
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// Асинхронно создает неизменяемый список <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Тип элементов неизменяемого списка</typeparam>
    /// <returns>Неизменяемый список <typeparamref name="T"/> элементов</returns>
    public static async Task<ImmutableList<T>> ToImmutableListAsync<T>(this IQueryable<T> query,
        CancellationToken cancellationToken = default)
        => ImmutableList.CreateRange(await query.ToArrayAsync(cancellationToken));
}