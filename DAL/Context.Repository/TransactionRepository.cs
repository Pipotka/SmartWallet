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

	Task<List<Transaction>> ITransactionRepository.GetListByMonthAndUserIdAsync(Guid userId, DateOnly monthOfYear, CancellationToken cancellationToken)
		=> storage.Read<Transaction>().NotDeleted()
		.Where(x => x.MadeAt.Month == monthOfYear.Month 
		&& x.MadeAt.Year == monthOfYear.Year).ToListAsync(cancellationToken);

	/// <inheritdoc/>
	public override void Add(Transaction entity)
	{
		entity.MadeAt = DateOnly.FromDateTime(DateTime.Now);
		base.Add(entity);
	}

	void ITransactionRepository.DeleteTransactionsBySpendingAreaId(Guid spendingAreaId)
		=> DeleteEverythingBy(e => e.ToSpendingAreaId == spendingAreaId);

	void ITransactionRepository.DeleteTransactionsByUserId(Guid userId)
		=> DeleteEverythingBy(e => e.UserId == userId);
}