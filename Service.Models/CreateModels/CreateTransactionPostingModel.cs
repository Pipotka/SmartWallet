namespace Nasurino.SmartWallet.Service.Models.CreateModels;

/// <summary>
/// Модель проводки для создания транзакции
/// </summary>
public sealed class CreateTransactionPostingModel
{
	/// <summary>
	/// Идентификатор счёта
	/// </summary>
	public Guid AccountId { get; set; }

	/// <summary>
	/// Сумма проводки со знаком
	/// </summary>
	public decimal Amount { get; set; }
}
