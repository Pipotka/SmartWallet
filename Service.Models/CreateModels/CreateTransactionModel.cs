namespace Nasurino.SmartWallet.Service.Models.CreateModels;

/// <summary>
/// Модель создания транзакции
/// </summary>
public class CreateTransactionModel
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public Guid UserId { get; set; }
	
	/// <summary>
	/// Идентификатор аккаунта-источника
	/// </summary>
	public Guid? SourceAccountId { get; set; }

	/// <summary>
	/// Идентификатор аккаунта назначения
	/// </summary>
	public Guid? DestinationAccountId { get; set; }

	/// <summary>
	/// Значение
	/// </summary>
	public double Amount { get; set; } = 0.0;
}