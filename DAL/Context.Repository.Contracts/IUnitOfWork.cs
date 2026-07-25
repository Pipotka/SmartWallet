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

		/// <inheritdoc cref="IDailyExpenseCategorieRepository"/>
		IDailyExpenseCategorieRepository DailyExpenseCategorieRepository { get; }

		/// <inheritdoc cref="IUserRepository"/>
		IUserRepository UserRepository { get; }

		/// <inheritdoc cref="IRefreshTokenRepository"/>
		IRefreshTokenRepository RefreshTokenRepository { get; }

		/// <summary>
		/// Сохраняет изменения
		/// </summary>
		Task SaveChangesAsync(CancellationToken cancellationToken);
	}
}
