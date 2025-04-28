using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="SpendingArea"/>
/// </summary>
public class SpendingAreaRepository(SmartWalletContext context) : BaseWriteRepository<SpendingArea>(context)
{
	/// <summary>
	/// Возвращает список областей трат идентификатору пользователя
	/// </summary>
	public Task<List<SpendingArea>> GetListSpendingAreaByUserIdAsync(Guid userId, CancellationToken cancellationToken)
		=> context.Set<SpendingArea>().AsNoTracking().Where(x => x.UserId == userId).ToListAsync(cancellationToken);

	/// <summary>
	/// Возвращает область трат по идентификатору
	/// </summary>
	public Task<SpendingArea?> GetSpendingAreaByIdAsync(Guid id, CancellationToken cancellationToken)
		=> context.Set<SpendingArea>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
