using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Repository.Specification;
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
	public Task<List<CashVault>> GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
		=> context.Set<CashVault>().AsNoTracking().NotDeleted().Where(x => x.UserId == userId).ToListAsync(cancellationToken);

	/// <summary>
	/// Возвращает денежное хранилище по идентификатору
	/// </summary>
	public Task<CashVault?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
		=> context.Set<CashVault>().AsNoTracking().NotDeleted().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
	
	/// <summary>
	/// Возвращает денежное хранилище по названию и идентификатору пользователя
	/// </summary>
	public Task<CashVault?> GetByNameAndUserIdAsync(Guid userId, string name, CancellationToken cancellationToken)
		=> context.Set<CashVault>().AsNoTracking().NotDeleted().Where(x => x.UserId == userId)
		.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken);

	/// <summary>
	/// Удаляет все денежные хранилища по идентификатору пользователя
	/// </summary>
	public void DeleteCashVaultsByUserId(Guid userId)
		=> DeleteEverythingBy(e => e.UserId == userId);
}
