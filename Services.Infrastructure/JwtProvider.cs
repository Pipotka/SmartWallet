using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Models.Models;
using Service.Infrastructure.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Nasurino.SmartWallet.Service.Infrastructure;

// [TODO] Регистрация проекта
/// <summary>
/// Провайдер JWT
/// </summary>
public class JwtProvider(IOptions<JwtOptions> options) : IJwtProvider
{
	private readonly JwtOptions options = options.Value;

	string IJwtProvider.GenerateToken(UserModel user)
	{
		var signingCredentials = new SigningCredentials(
			new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
			SecurityAlgorithms.HmacSha256);

		var claims = new List<Claim>
		{
			new (JwtRegisteredClaimNames.NameId, user.Id.ToString()),
		};

		var token = new JwtSecurityToken(
			claims: claims,
			signingCredentials: signingCredentials,
			expires: DateTime.UtcNow.AddHours(options.ExpiresHours));

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
