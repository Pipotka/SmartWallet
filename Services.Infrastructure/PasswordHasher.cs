using Bc = BCrypt.Net;

namespace Nasurino.SmartWallet.Service.Infrastructure;

/// <summary>
/// Хэшер паролей
/// </summary>
public class PasswordHasher : IPasswordHasher
{
	string IPasswordHasher.Generate(string password)
		=> Bc.BCrypt.HashPassword(password);

	bool IPasswordHasher.Verify(string password, string hashedPassword)
		=> Bc.BCrypt.Verify(password, hashedPassword);
}