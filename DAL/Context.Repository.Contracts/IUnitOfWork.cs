namespace Nasurino.SmartWallet.Context.Repository.Contracts
{
	/// <summary>
	/// Интерфейс UnitOfWork
	/// </summary>
	public interface IUnitOfWork
	{
		/// <inheritdoc cref="TransactionEndpointRepository"/>
		ITransactionEndpointRepository TransactionEndpointRepository { get; }

		/// <inheritdoc cref="ITransactionRepository"/>
		ITransactionRepository TransactionRepository { get; }

		/// <inheritdoc cref="IUserRepository"/>
		IUserRepository UserRepository { get; }

		/// <summary>
		/// Сохраняет изменения
		/// </summary>
		Task SaveChangesAsync(CancellationToken cancellationToken);
	}
}