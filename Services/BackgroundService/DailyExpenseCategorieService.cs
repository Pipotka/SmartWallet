using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Services.Contracts.BackgroundService;

namespace Nasurino.SmartWallet.Services.BackgroundService;

/// <summary>
/// Сервис пересчёта агрегатов дневных трат (<see cref="DailyExpenseCategorie"/>) по триггеру
/// создания/удаления транзакции.
/// </summary>
public class DailyExpenseCategorieService(ITransactionRepository transactionRepository) : IDailyExpenseCategorieService
{
	Task IDailyExpenseCategorieService.RecalculateForTransactionAsync(Transaction transaction, CancellationToken cancellationToken)
		=> transactionRepository.RecalculateDailyExpenseCategoriesAsync(transaction, cancellationToken);
}
