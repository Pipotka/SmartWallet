using Nasurino.SmartWallet.Entities.Contracts;

namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Базавоя сущность
/// </summary>
public abstract class BaseEntity : IBaseEntity
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }
}