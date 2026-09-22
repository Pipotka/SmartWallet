namespace Nasurino.SmartWallet.Services.Models.Models;

/// <summary>
/// Модель смены пароля
/// </summary>
public class ChangePasswordModel
{
    /// <summary>
    /// Id пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Старый пароль
    /// </summary>
    public string OldPassword { get; set; }

    /// <summary>
    /// Новый пароль
    /// </summary>
    public string NewPassword { get; set; }
}