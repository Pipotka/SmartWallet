namespace Nasurino.SmartWallet.Context.Repository.Contracts
{
	/// <summary>
	/// Интерфейс UnitOfWork
	/// </summary>
	public interface IUnitOfWork
	{
		/// <inheritdoc cref="ICashVaultRepository"/>
		ICashVaultRepository CashVaultRepository { get; }

		/// <inheritdoc cref="ISpendingAreaRepository"/>
		ISpendingAreaRepository SpendingAreaRepository { get; }

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