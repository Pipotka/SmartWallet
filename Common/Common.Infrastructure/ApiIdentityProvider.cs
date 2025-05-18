using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using System.Security.Claims;

namespace Nasurino.SmartWallet.Common.Infrastructure;

/// <summary>
/// Поставщик опознания личности
/// </summary>
public class ApiIdentityProvider : IIdentityProvider
{
	private IEnumerable<Claim> claims;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="ApiIdentityProvider"/>
	/// </summary>
	public ApiIdentityProvider(IHttpContextAccessor httpContextAccessor)
	{
		claims = httpContextAccessor?.HttpContext?.User.Claims ?? [];
	}

	/// <inheritdoc/>
	public Guid Id => Guid.TryParse(claims
		.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value, out var value)
		? value : Guid.Empty;
}
