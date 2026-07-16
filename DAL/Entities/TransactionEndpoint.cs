namespace Nasurino.SmartWallet.Entities;

/// <summary>
/// Конечная точка потока денег
/// </summary>
public class TransactionEndpoint : SmartDeletedEntity
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Навигационное свойство
	/// </summary>
	public User? User { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; set; } = string.Empty;
	
	/// <summary>
	/// Ограничение трат
	/// </summary>
	public double? Limitation { get; set; }
	
	/// <summary>
	/// Флаг указывающий, что конечная точка является денежным хранилищем
	/// </summary>
	public bool IsStorage { get; set; }
	
	/// <summary>
	/// Значение
	/// </summary>
	public double Value { get; set; } = 0.0;

	/// <summary>
	/// Записи движения средств по текущей точке
	/// </summary>
	public ICollection<Posting> Postings { get; set; } = new List<Posting>();

	/// <summary>
	/// Агрегаты дневных трат по текущей области трат
	/// </summary>
	public ICollection<DailyExpenseCategorie> DailyExpenseCategories { get; set; } = new List<DailyExpenseCategorie>();
}
