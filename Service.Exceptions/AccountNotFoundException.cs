using Microsoft.AspNetCore.Http;

namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка сервиса. Счёт не найден
/// </summary>
public sealed class AccountNotFoundException : ServiceException
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="AccountNotFoundException"/>
	/// </summary>
	public AccountNotFoundException(Guid accountId)
		: base($"Счет {accountId} не найден", ErrorCodes.AccountNotFound, StatusCodes.Status404NotFound)
	{
	}
}
