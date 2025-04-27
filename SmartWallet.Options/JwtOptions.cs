namespace Nasurino.SmartWallet.Options;

/// <summary>
/// Найстройки конфигурацииы для Jwt
/// </summary>
public class JwtOptions
{
	/// <summary>
	/// Ключ для генерации Jwt
	/// </summary>
	public string Key { get; set; }

	/// <summary>
	/// Количество часов, которое действует Jwt
	/// </summary>
	public int ExpiresHours { get; set; }
}
