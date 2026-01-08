using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Specification;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="Transaction"/>
/// </summary>
public sealed class TransactionRepository : BaseWriteRepository<Transaction>, ITransactionRepository
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="TransactionRepository"/>
	/// </summary>
	public TransactionRepository(IDataStorageContext storage) : base(storage)
	{ }

	Task<List<Transaction>> ITransactionRepository.GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
		=> Storage.Read<Transaction>().NotDeleted().Where(x => x.UserId == userId).ToListAsync(cancellationToken);

	Task<Transaction?> ITransactionRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
		=> Storage.Read<Transaction>().NotDeleted().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

	Task<Transaction?> ITransactionRepository.GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
		=> Storage.Read<Transaction>().NotDeleted().FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

	async Task<IReadOnlyCollection<Transaction>> ITransactionRepository.GetListByDateRangeAndUserIdAsync(Guid userId,
		DateTime startTimeRange,
		DateTime endTimeRange,
		CancellationToken cancellationToken)
		=> await Storage.Read<Transaction>().NotDeleted()
		.Where(x => InDateRange(x,  startTimeRange, endTimeRange)
			&& x.UserId == userId)
		.ToListAsync(cancellationToken);

	/// <inheritdoc/>
	public override void Add(Transaction entity)
	{
		entity.MadeAt = DateTime.UtcNow;
		base.Add(entity);
	}

	void ITransactionRepository.DeleteTransactionsByTransactionEndpointIdAndDateRange(Guid transactionEndpointId,
		DateTime startDate = default,
		DateTime endDate = default)
	{
		endDate = endDate == default ? DateTime.MaxValue : endDate;
		DeleteEverythingBy(e => (e.DestinationAccountId == transactionEndpointId || e.SourceAccountId == transactionEndpointId) 
		                        && InDateRange(e,  startDate, endDate));
	}

	void ITransactionRepository.DeleteTransactionsByUserId(Guid userId)
		=> DeleteEverythingBy(e => e.UserId == userId);
	
	async Task<IReadOnlyCollection<Transaction>> ITransactionRepository.GetListExpenseByDateRangeAndUserIdAsync(Guid userId,
		DateTime startDate,
		DateTime endDate,
		CancellationToken cancellationToken)
		=> await Storage.Read<Transaction>().NotDeleted()
			.Where(x => InDateRange(x,  startDate, endDate)
				&& x.UserId == userId
				&& x.DestinationAccount != null && !x.DestinationAccount.IsStorage)
			.ToListAsync(cancellationToken);

	async Task<double> ITransactionRepository.GetBalanceByAccountIdAndDateRangeAsync(Guid accountId,
		CancellationToken cancellationToken,
		DateTime startDate = default,
		DateTime endDate = default)
	{
		endDate = endDate == default ? DateTime.MaxValue : endDate;
		
		return await Storage.Read<Transaction>().NotDeleted()
			.Where(x => InDateRange(x, startDate, endDate))
			.Where(x => (x.SourceAccountId != null && x.SourceAccountId == accountId)
			            || (x.DestinationAccountId != null && x.DestinationAccountId == accountId))
			.SumAsync(x => x.SourceAccountId == accountId ? -x.Amount : x.Amount, cancellationToken);
	}

	private bool InDateRange(Transaction transaction,
		DateTime startTimeRange,
		DateTime endTimeRange)
		=> startTimeRange <= transaction.MadeAt && transaction.MadeAt < endTimeRange;
}