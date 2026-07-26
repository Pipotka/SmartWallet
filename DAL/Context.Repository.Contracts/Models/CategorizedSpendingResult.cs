namespace Nasurino.SmartWallet.Context.Repository.Contracts.Models;

/// <summary>
/// Результат категоризации трат пользователя
/// </summary>
public sealed class CategorizedSpendingResult
	{
		/// <summary>
		/// Общая сумма всех трат по категориям
		/// </summary>
		public decimal TotalSpending { get; set; }
	
		/// <summary>
		/// Категории с суммами трат
		/// </summary>
		public IReadOnlyCollection<CategorySpendingItem> Categories { get; set; }
	}