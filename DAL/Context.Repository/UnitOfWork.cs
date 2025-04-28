namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Паттерн единица работы
/// </summary>
public class UnitOfWork(SmartWalletContext context)
{
	private readonly SmartWalletContext context = context;

	/// <inheritdoc cref="Repository.UserRepository"/>
	public UserRepository UserRepository { get; set; } = new(context);

	/// <inheritdoc cref="Repository.CashVaultRepository"/>
	public CashVaultRepository CashVaultRepository { get; set; } = new(context);

	/// <inheritdoc cref="Repository.TransactionRepository"/>
	public TransactionRepository TransactionRepository { get; set; } = new(context);

	/// <inheritdoc cref="Repository.SpendingAreaRepository"/>
	public SpendingAreaRepository SpendingAreaRepository { get; set; } = new(context);

	/// <summary>
	/// Сохраняет изменения
	/// </summary>
	public Task SaveChangesAsync(CancellationToken cancellationToken)
		=> context.SaveChangesAsync(cancellationToken);
}
