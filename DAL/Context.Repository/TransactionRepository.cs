using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Specification;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="Transaction"/>
/// </summary>
public class TransactionRepository(IDataStorageContext storage) : BaseWriteRepository<Transaction>(storage), ITransactionRepository
{
	Task<List<Transaction>> ITransactionRepository.GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
		=> storage.Read<Transaction>().NotDeleted().Where(x => x.UserId == userId).ToListAsync(cancellationToken);

	Task<Transaction?> ITransactionRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
		=> storage.Read<Transaction>().NotDeleted().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

	Task<Transaction?> ITransactionRepository.GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
		=> storage.Read<Transaction>().NotDeleted().FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

	async Task<IReadOnlyCollection<Transaction>> ITransactionRepository.GetListByTimeRangeAndUserIdAsync(Guid userId,
		DateTime startTimeRange,
		DateTime endTimeRange,
		CancellationToken cancellationToken)
		=> await storage.Read<Transaction>().NotDeleted()
		.Where(x => startTimeRange <= x.MadeAt && x.MadeAt < endTimeRange)
		.ToListAsync(cancellationToken);

	/// <inheritdoc/>
	public override void Add(Transaction entity)
	{
		entity.MadeAt = DateTime.UtcNow;
		base.Add(entity);
	}

	void ITransactionRepository.DeleteTransactionsBySpendingAreaId(Guid spendingAreaId)
		=> DeleteEverythingBy(e => e.ToSpendingAreaId == spendingAreaId);

	void ITransactionRepository.DeleteTransactionsByUserId(Guid userId)
		=> DeleteEverythingBy(e => e.UserId == userId);
}