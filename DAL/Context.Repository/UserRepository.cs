using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="User"/>
/// </summary>
public class UserRepository(SmartWalletContext context) : BaseWriteRepository<User>(context)
{
	/// <summary>
	/// Возвращает пользователя по электронной почте
	/// </summary>
	public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
		=> context.Set<User>().AsNoTracking().FirstOrDefaultAsync(x => x.Email == email.ToLower(), cancellationToken);

	/// <summary>
	/// Возвращает пользователя по идентификатору
	/// </summary>
	public Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
		=> context.Set<User>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}