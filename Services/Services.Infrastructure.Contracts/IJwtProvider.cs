using Nasurino.SmartWallet.Services.Models.Models;

namespace Nasurino.SmartWallet.Services.Infrastructure.Contracts
{
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
}