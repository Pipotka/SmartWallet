using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Specification;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="TransactionEndpoint"/>
/// </summary>
public class TransactionEndpointRepository(IDataStorageContext storage) : BaseWriteRepository<TransactionEndpoint>(storage), ITransactionEndpointRepository
{
	Task<List<TransactionEndpoint>> ITransactionEndpointRepository.GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
		=> Storage.Read<TransactionEndpoint>()
			.NotDeleted()
			.Where(x => x.UserId == userId)
			.OrderByDescending(x => x.IsStorage)
			.ToListAsync(cancellationToken);

	Task<TransactionEndpoint?> ITransactionEndpointRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
		=> Storage.Read<TransactionEndpoint>().NotDeleted().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

	Task<TransactionEndpoint?> ITransactionEndpointRepository.GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
		=> Storage.Read<TransactionEndpoint>().NotDeleted().FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

	Task<TransactionEndpoint?> ITransactionEndpointRepository.GetByNameAndUserIdAsync(Guid userId, string name, CancellationToken cancellationToken)
		=> Storage.Read<TransactionEndpoint>().NotDeleted().Where(x => x.UserId == userId)
		.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken);

	void ITransactionEndpointRepository.DeleteTransactionEndpointsByUserId(Guid userId)
		=> DeleteEverythingBy(e => e.UserId == userId);
}
