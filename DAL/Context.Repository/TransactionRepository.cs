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
	public Task<List<Transaction>> GetListTransactionByUserId(Guid userId, CancellationToken cancellationToken)
		=> context.Set<Transaction>().AsNoTracking().Where(x => x.UserId == userId).ToListAsync(cancellationToken);

	/// <inheritdoc/>
	public override void Add(Transaction entity)
	{
		entity.MadeAt = DateTime.UtcNow;
		base.Add(entity);
	}
}