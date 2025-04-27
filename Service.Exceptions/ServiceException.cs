namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка сервиса
/// </summary>
public abstract class ServiceException : Exception
{
	protected ServiceException(string message)
		: base(message)
	{

	}
}
