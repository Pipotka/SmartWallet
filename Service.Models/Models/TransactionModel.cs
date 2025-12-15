namespace Nasurino.SmartWallet.Service.Models.Models;

/// <summary>
/// Модель транзакции
/// </summary>
public class TransactionModel
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }

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

	/// <summary>
	/// Дата создания
	/// </summary>
	public DateTime MadeAt { get; set; }
}