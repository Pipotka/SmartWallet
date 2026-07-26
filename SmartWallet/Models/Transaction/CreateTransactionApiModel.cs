namespace Nasurino.SmartWallet.Models.Transaction;

/// <summary>
/// Api модель создания транзакции
/// </summary>
public sealed class CreateTransactionApiModel
{
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
	public decimal Amount { get; set; } = 0.0m;
}