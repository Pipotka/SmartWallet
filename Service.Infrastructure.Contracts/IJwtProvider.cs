using Nasurino.SmartWallet.Service.Models;

namespace Nasurino.SmartWallet.Service.Infrastructure;

/// <summary>
/// Интерфейс провайдера JWT
/// </summary>
public interface IJwtProvider
{
	/// <summary>
	/// Генерирует JWT
	/// </summary>
	string GenerateToken(UserModel user);
}