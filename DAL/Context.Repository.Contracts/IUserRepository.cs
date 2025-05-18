using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts
{
	/// <summary>
	/// Репозиторий для работы с <see cref="User"/>
	/// </summary>
	public interface IUserRepository : IBaseWriteRepository<User>
	{
		/// <summary>
		/// Возвращает пользователя по электронной почте
		/// </summary>
		Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
		
		/// <summary>
		/// Возвращает пользователя по идентификатору
		/// </summary>
		Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);
	}
}