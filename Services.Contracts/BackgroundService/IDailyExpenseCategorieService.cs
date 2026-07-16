using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Services.Contracts.BackgroundService
{
	/// <summary>
	/// Интерфейс фонового сервиса пересчёта агрегатов дневных трат
	/// </summary>
	public interface IDailyExpenseCategorieService
	{
		/// <summary>
		/// Пересчитывает агрегаты дневных трат для областей трат, затронутых транзакцией
		/// </summary>
		/// <param name="transaction">Транзакция, изменившая траты</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task RecalculateForTransactionAsync(Transaction transaction, CancellationToken cancellationToken);
	}
}
