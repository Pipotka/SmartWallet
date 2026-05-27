namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Рефреш-токен
/// </summary>
public class RefreshToken : BaseEntity
{
	/// <summary>
	/// Значение токена
	/// </summary>
	public string Token { get; set; }

	/// <summary>
	/// Идентификатор пользователя (FK)
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Дата истечения срока действия
	/// </summary>
	public DateTime ExpiresAt { get; set; }

	/// <summary>
	/// Дата создания
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// Дата отзыва (null = активный)
	/// </summary>
	public DateTime? RevokedAt { get; set; }

	/// <summary>
	/// Токен, заменивший данный
	/// </summary>
	public string? ReplacedByToken { get; set; }

	/// <summary>
	/// Навигационное свойство к пользователю
	/// </summary>
	public User User { get; set; }
}
