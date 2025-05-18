namespace Nasurino.SmartWallet.Common.Infrastructure.Contracts;

/// <summary>
/// Базовая функциональность идентификации пользователя
/// </summary>
public interface IIdentityProvider
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	Guid Id { get; }
}
