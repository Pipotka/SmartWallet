using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="CashVault"/>
/// </summary>
public class CashVaultRepository(SmartWalletContext context) : BaseWriteRepository<CashVault>(context)
{
	/// <summary>
	/// Возвращает список денежных хранилищ по идентификатору пользователя
	/// </summary>
	public Task<List<CashVault>> GetListCashVaultByUserId(Guid userId, CancellationToken cancellationToken)
		=> context.Set<CashVault>().AsNoTracking().Where(x => x.UserId == userId).ToListAsync(cancellationToken);
}
