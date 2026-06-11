using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="RefreshToken"/>
/// </summary>
public class RefreshTokenRepository(IDataStorageContext storage) : BaseWriteRepository<RefreshToken>(storage), IRefreshTokenRepository
{
	Task<RefreshToken?> IRefreshTokenRepository.GetByTokenAsync(string token, CancellationToken cancellationToken)
		=> Storage.Read<RefreshToken>().FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

	async Task<IReadOnlyCollection<RefreshToken>> IRefreshTokenRepository.GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
	{
		var tokens = await Storage.Read<RefreshToken>()
			.Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow)
			.ToListAsync(cancellationToken);
		return tokens;
	}
}
