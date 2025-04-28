namespace Nasurino.SmartWallet.Entities.Contracts;

/// <summary>
/// Сущность с фукцией "мягкого" удаления
/// </summary>
public interface ISmartDeletedEntity
{
	/// <summary>
	/// Дата удаления
	/// </summary>
	DateTime? DeletedAt { get; set; }
}