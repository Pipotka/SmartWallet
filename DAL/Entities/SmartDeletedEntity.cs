namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Сущность с функцией "мягкого" удаления
/// </summary>
public abstract class SmartDeletedEntity : BaseEntity
{
	/// <summary>
	/// Дата удаления
	/// </summary>
	public DateTime? DeletedAt { get; set; }
}