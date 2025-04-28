using Nasurino.SmartWallet.Entities.Contracts;

namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Сущность с функцией "мягкого" удаления
/// </summary>
public abstract class SmartDeletedEntity : BaseEntity, ISmartDeletedEntity
{
	/// <summary>
	/// Дата удаления
	/// </summary>
	public DateTime? DeletedAt { get; set; }
}