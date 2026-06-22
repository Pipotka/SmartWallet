using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts
{
	/// <summary>
	/// Репозиторий для работы с <see cref="RefreshToken"/>
	/// </summary>
	public interface IRefreshTokenRepository : IBaseWriteRepository<RefreshToken>
	{
		/// <summary>
		/// Возвращает рефреш-токен по значению токена
		/// </summary>
		Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает все активные рефреш-токены пользователя
		/// </summary>
		Task<IReadOnlyCollection<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
	}
}
