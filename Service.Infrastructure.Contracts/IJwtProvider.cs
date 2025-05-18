using Nasurino.SmartWallet.Service.Models.Models;

namespace Service.Infrastructure.Contracts
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