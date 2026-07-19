namespace Nasurino.SmartWallet.Service.Models.Models;

/// <summary>
/// Модель проводки (движения средств по счёту) без полей мягкого удаления и даты создания
/// </summary>
public class PostingModel
{
	/// <summary>
	/// Идентификатор проводки
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Идентификатор счёта
	/// </summary>
	public Guid AccountId { get; set; }

	/// <summary>
	/// Идентификатор транзакции
	/// </summary>
	public Guid TransactionId { get; set; }

	/// <summary>
	/// Сумма проводки со знаком (минус — расход, плюс — приход)
	/// </summary>
	public decimal Amount { get; set; }
}
