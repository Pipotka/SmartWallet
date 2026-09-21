namespace Nasurino.SmartWallet.Service.Models.CreateModels;

public sealed class CreateTransactionPostingModel
{
    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }
}
