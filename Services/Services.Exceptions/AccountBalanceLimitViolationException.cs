namespace Nasurino.SmartWallet.Services.Exceptions;

/// <summary>
/// Ошибка сервиса. Нарушения лимита аккаунта
/// </summary>
public sealed class AccountBalanceLimitViolationException : EntityServiceException
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="AccountBalanceLimitViolationException"/>
    /// </summary>
    public AccountBalanceLimitViolationException(string fielName, string endpoinName) 
        : base($"{fielName} - {endpoinName} вышел за установленный лимит")
    {
    }
}