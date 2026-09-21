namespace Nasurino.SmartWallet.Models.Transaction;

/// <summary>
/// Api модель создания транзакции
/// </summary>
public sealed class CreateTransactionApiModel
{
	public List<CreateTransactionPostingApiModel> Postings { get; set; } = [];
}
