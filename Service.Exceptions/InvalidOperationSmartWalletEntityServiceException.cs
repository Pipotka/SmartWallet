namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка сервиса. Недопустимая операция с сущностью
/// </summary>
public class InvalidOperationSmartWalletEntityServiceException : EntityServiceException
{
	public InvalidOperationSmartWalletEntityServiceException(string message) 
		: base(message)
	{

	}
}
