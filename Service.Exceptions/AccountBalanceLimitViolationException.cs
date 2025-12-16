namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка сервиса. Нарушения лимита аккаунта
/// </summary>
public sealed class AccountBalanceLimitViolationException : EntityServiceException
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="AccountBalanceLimitViolationException"/>
    /// </summary>
    public AccountBalanceLimitViolationException(Guid transactionEndpointId) 
        : base($"Баланс конечной точки транзакции c id = {transactionEndpointId} вышел за установленный лимит")
    {
    }
}