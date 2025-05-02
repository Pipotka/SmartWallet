namespace Nasurino.SmartWallet.Service.Models.DeleteModels;

/// <summary>
/// Модель удаления области трат
/// </summary>
public class DeleteSpendingAreaModel
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Флаг, указывающий нужно ли удалить все транзакции связанные с данной областью трат
	/// </summary>
	public bool IsDeleteAllRelatedTransactions { get; set; } = false;
}