namespace Nasurino.SmartWallet.Entities.Contracts;

/// <summary>
/// Сущность с фукцией "мягкого" удаления
/// </summary>
public interface ISmartDeletedEntity
{
	/// <summary>
	/// Дата удаления
	/// </summary>
	DateTimeOffset? DeletedAt { get; set; }
}