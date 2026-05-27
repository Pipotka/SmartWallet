namespace Nasurino.SmartWallet.Models.Account;

/// <summary>
/// Api модель ответа обновления токена
/// </summary>
public class ResponseRefreshApiModel
{
	/// <summary>
	/// Access-токен (JWT)
	/// </summary>
	public string AccessToken { get; set; }
}
