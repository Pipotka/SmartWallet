namespace Nasurino.SmartWallet.Options;

/// <summary>
/// Настройки конфигурации для Jwt
/// </summary>
public class JwtOptions
{
	/// <summary>
	/// Ключ для генерации Jwt
	/// </summary>
	public string Key { get; set; }

	/// <summary>
	/// Количество минут, которое действует access-токен
	/// </summary>
	public int ExpiresMinutes { get; set; }

	/// <summary>
	/// Количество дней, которое действует refresh-токен
	/// </summary>
	public int RefreshExpiresDays { get; set; }
}
