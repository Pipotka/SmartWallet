namespace Nasurino.SmartWallet.Context.Repository.Contracts
{
	/// <summary>
	/// Интерфейс UnitOfWork
	/// </summary>
	public interface IUnitOfWork
	{
		/// <inheritdoc cref="ICashVaultRepository"/>
		ICashVaultRepository CashVaultRepository { get; set; }

		/// <inheritdoc cref="ISpendingAreaRepository"/>
		ISpendingAreaRepository SpendingAreaRepository { get; set; }

		/// <inheritdoc cref="ITransactionRepository"/>
		ITransactionRepository TransactionRepository { get; set; }

		/// <inheritdoc cref="IUserRepository"/>
		IUserRepository UserRepository { get; set; }

		/// <summary>
		/// Сохраняет изменения
		/// </summary>
		Task SaveChangesAsync(CancellationToken cancellationToken);
	}
}