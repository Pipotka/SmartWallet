using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="Transaction"/>
/// </summary>
public class TransactionRepository(SmartWalletContext context) : BaseWriteRepository<Transaction>(context)
{
	/// <summary>
	/// Возвращает список транзакций по идентификатору пользователя
	/// </summary>
	public Task<List<Transaction>> GetListTransactionByUserIdAsync(Guid userId, CancellationToken cancellationToken)
		=> context.Set<Transaction>().AsNoTracking().Where(x => x.UserId == userId).ToListAsync(cancellationToken);

	/// <summary>
	/// Возвращает транзакцию по идентификатору
	/// </summary>
	public Task<Transaction?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken)
		=> context.Set<Transaction>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

	/// <inheritdoc/>
	public override void Add(Transaction entity)
	{
		entity.MadeAt = DateTime.UtcNow;
		base.Add(entity);
	}

	/// <summary>
	/// Удаляет все транзакций по идентификатору пользователя
	/// </summary>
	public void DeleteTransactionsByUserId(Guid userId)
		=> DeleteEverythingBy(e => e.UserId == userId);
}