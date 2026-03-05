using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Models.Transaction;

/// <summary>
/// Api модель транзакции
/// </summary>
public sealed class TransactionApiModel
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
	/// Тип транзакции
	/// </summary>
	public TransactionType Type { get; set; }

	/// <summary>
	/// Значение
	/// </summary>
	public double Amount { get; set; } = 0.0;

	/// <summary>
	/// Дата создания
	/// </summary>
	public DateTime MadeAt { get; set; }
}