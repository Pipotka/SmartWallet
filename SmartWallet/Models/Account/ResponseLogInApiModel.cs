namespace Nasurino.SmartWallet.Models.Account;

/// <summary>
/// Api модель ответа входа пользователя
/// </summary>
public class ResponseLogInApiModel
{
	/// <summary>
	/// Access-токен (JWT)
	/// </summary>
	public string AccessToken { get; set; }
}
