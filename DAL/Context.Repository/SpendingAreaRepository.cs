using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Specification;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="SpendingArea"/>
/// </summary>
public class SpendingAreaRepository(IDataStorageContext storage) : BaseWriteRepository<SpendingArea>(storage), ISpendingAreaRepository
{
	Task<List<SpendingArea>> ISpendingAreaRepository.GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
		=> storage.Read<SpendingArea>().NotDeleted().Where(x => x.UserId == userId).ToListAsync(cancellationToken);

	Task<SpendingArea?> ISpendingAreaRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
		=> storage.Read<SpendingArea>().NotDeleted().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

	Task<SpendingArea?> ISpendingAreaRepository.GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
		=> storage.Read<SpendingArea>().NotDeleted().FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

	Task<SpendingArea?> ISpendingAreaRepository.GetByNameAndUserIdAsync(Guid userId, string name, CancellationToken cancellationToken)
		=> storage.Read<SpendingArea>().NotDeleted().Where(x => x.UserId == userId)
		.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken);

	void ISpendingAreaRepository.DeleteSpendingAreasByUserId(Guid userId)
		=> DeleteEverythingBy(e => e.UserId == userId);
}
