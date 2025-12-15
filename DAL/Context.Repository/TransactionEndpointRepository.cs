using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Specification;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="TransactionEndpoint"/>
/// </summary>
public class TransactionEndpointRepository(IDataStorageContext storage) : BaseWriteRepository<TransactionEndpoint>(storage), Contracts.ITransactionEndpointRepository
{
	Task<List<TransactionEndpoint>> Contracts.ITransactionEndpointRepository.GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
		=> storage.Read<TransactionEndpoint>()
			.NotDeleted()
			.Where(x => x.UserId == userId)
			.OrderByDescending(x => x.IsStorage)
			.ToListAsync(cancellationToken);

	Task<TransactionEndpoint?> Contracts.ITransactionEndpointRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
		=> storage.Read<TransactionEndpoint>().NotDeleted().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

	Task<TransactionEndpoint?> Contracts.ITransactionEndpointRepository.GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
		=> storage.Read<TransactionEndpoint>().NotDeleted().FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

	Task<TransactionEndpoint?> Contracts.ITransactionEndpointRepository.GetByNameAndUserIdAsync(Guid userId, string name, CancellationToken cancellationToken)
		=> storage.Read<TransactionEndpoint>().NotDeleted().Where(x => x.UserId == userId)
		.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken);

	void Contracts.ITransactionEndpointRepository.DeleteTransactionEndpointsByUserId(Guid userId)
		=> DeleteEverythingBy(e => e.UserId == userId);
}
