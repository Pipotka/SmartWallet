namespace Nasurino.SmartWallet.Models.Transaction;

public sealed class CreateTransactionPostingApiModel
{
    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }
}
