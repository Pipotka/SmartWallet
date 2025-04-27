namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Ошибка валидации
/// </summary>
public class SmartWalletValidationException : EntityServiceException
{
	public SmartWalletValidationException(string message)
		: base(message)
	{

	}
}
