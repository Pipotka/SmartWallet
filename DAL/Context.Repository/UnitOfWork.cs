using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Паттерн единица работы
/// </summary>
public class UnitOfWork(IDataStorageContext storage) : IUnitOfWork
{
	private readonly IDataStorageContext storage = storage;

	IUserRepository IUnitOfWork.UserRepository { get; set; } = new UserRepository(storage);

	ICashVaultRepository IUnitOfWork.CashVaultRepository { get; set; } = new CashVaultRepository(storage);

	ITransactionRepository IUnitOfWork.TransactionRepository { get; set; } = new TransactionRepository(storage);

	ISpendingAreaRepository IUnitOfWork.SpendingAreaRepository { get; set; } = new SpendingAreaRepository(storage);

	Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
		=> storage.SaveChangesAsync(cancellationToken);
}
