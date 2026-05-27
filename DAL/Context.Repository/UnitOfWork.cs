using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Паттерн единица работы
/// </summary>
public class UnitOfWork : IUnitOfWork
{
	private readonly IDataStorageContext storage;

	public IUserRepository UserRepository { get; init; }

	public ITransactionEndpointRepository TransactionEndpointRepository { get; init; }

	public ITransactionRepository TransactionRepository { get; init; }

	public IRefreshTokenRepository RefreshTokenRepository { get; init; }

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="UnitOfWork"/>
	/// </summary>
	public UnitOfWork(IDataStorageContext storage)
	{
		this.storage =  storage;

		UserRepository = new UserRepository(storage);
		TransactionEndpointRepository = new TransactionEndpointRepository(storage);
		TransactionRepository = new TransactionRepository(storage);
		RefreshTokenRepository = new RefreshTokenRepository(storage);
	}

	Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
		=> storage.SaveChangesAsync(cancellationToken);
}
