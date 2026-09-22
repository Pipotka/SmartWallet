using Microsoft.Extensions.Options;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Services.Infrastructure.Contracts;
using Bc = BCrypt.Net;

namespace Nasurino.SmartWallet.Services.Infrastructure;

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