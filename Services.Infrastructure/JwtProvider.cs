using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Nasurino.SmartWallet.Service.Infrastructure;

/// <summary>
/// Провайдер JWT
/// </summary>
public class JwtProvider(IOptions<JwtOptions> options)
{
	private readonly JwtOptions options = options.Value;

	/// <summary>
	/// Генерирует JWT
	/// </summary>
	public string GenerateToken(UserModel user)
	{
		var signingCredentials = new SigningCredentials(
			new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
			SecurityAlgorithms.HmacSha256);

		Claim[] claims = [new("userId", user.Id.ToString())];

		var token = new JwtSecurityToken(
			claims: claims,
			signingCredentials: signingCredentials,
			expires: DateTime.UtcNow.AddHours(options.ExpiresHours));

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
