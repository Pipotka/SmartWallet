using Microsoft.AspNetCore.Http;

namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка валидации проводок транзакции
/// </summary>
public sealed class PostingsValidationException : ServiceException
{
	/// <summary>
	/// Инициализирует новый экземпляр <see cref="PostingsValidationException"/>
	/// </summary>
	public PostingsValidationException(string errorCode, string message)
		: base(message, errorCode, StatusCodes.Status400BadRequest)
	{
	}
}
