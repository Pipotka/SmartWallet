using Bc = BCrypt.Net;

namespace Nasurino.SmartWallet.Service.Infrastructure;

/// <summary>
/// Хэшер паролей
/// </summary>
public static class PasswordHasher
{
	/// <summary>
	/// Генерирует хэш для пароля
	/// </summary>
	public static string Generate(string password)
		=> Bc.BCrypt.HashPassword(password);

	/// <summary>
	/// Проверяет эквивалентность пароля и хэша пароля
	/// </summary>
	public static bool Verify(string password, string hashedPassword)
		=> Bc.BCrypt.Verify(password, hashedPassword);
}