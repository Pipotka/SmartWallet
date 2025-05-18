using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Specification;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="CashVault"/>
/// </summary>
public class CashVaultRepository(IDataStorageContext storage) : BaseWriteRepository<CashVault>(storage), ICashVaultRepository
{
	Task<List<CashVault>> ICashVaultRepository.GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
		=> storage.Read<CashVault>().AsNoTracking().NotDeleted().Where(x => x.UserId == userId).ToListAsync(cancellationToken);

	Task<CashVault?> ICashVaultRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
		=> storage.Read<CashVault>().AsNoTracking().NotDeleted().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

	Task<CashVault?> ICashVaultRepository.GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
		=> storage.Read<CashVault>().AsNoTracking().NotDeleted().FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

	Task<CashVault?> ICashVaultRepository.GetByNameAndUserIdAsync(Guid userId, string name, CancellationToken cancellationToken)
		=> storage.Read<CashVault>().AsNoTracking().NotDeleted().Where(x => x.UserId == userId)
		.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken);

	void ICashVaultRepository.DeleteCashVaultsByUserId(Guid userId)
		=> DeleteEverythingBy(e => e.UserId == userId);
}
