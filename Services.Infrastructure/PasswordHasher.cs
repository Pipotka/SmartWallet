using Microsoft.Extensions.Options;
using Nasurino.SmartWallet.Options;
using Bc = BCrypt.Net;

namespace Nasurino.SmartWallet.Service.Infrastructure;

/// <summary>
/// Хэшер паролей
/// </summary>
public class PasswordHasher(IOptions<BCryptOptions> options) : IPasswordHasher
{
    private readonly BCryptOptions options = options.Value;

    string IPasswordHasher.Generate(string password)
		=> Bc.BCrypt.HashPassword(password, workFactor: options.WorkFactor);

	bool IPasswordHasher.Verify(string password, string hashedPassword)
		=> Bc.BCrypt.Verify(password, hashedPassword);
}