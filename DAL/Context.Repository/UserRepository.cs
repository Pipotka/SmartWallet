using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Specification;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="User"/>
/// </summary>
public class UserRepository(IDataStorageContext storage) : BaseWriteRepository<User>(storage), IUserRepository
{
	Task<User?> IUserRepository.GetUserByEmailAsync(string email, CancellationToken cancellationToken)
		=> Storage.Read<User>().NotDeleted().FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

	Task<User?> IUserRepository.GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
		=> Storage.Read<User>().NotDeleted().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}