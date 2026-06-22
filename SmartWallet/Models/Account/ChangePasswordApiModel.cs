namespace Nasurino.SmartWallet.Models.Account;

/// <summary>
/// Api модель смены пароля
/// </summary>
public class ChangePasswordApiModel
{
    /// <summary>
    /// Старый пароль
    /// </summary>
    public string OldPassword { get; set; }

    /// <summary>
    /// Новый пароль
    /// </summary>
    public string NewPassword { get; set; }
}