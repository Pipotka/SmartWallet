using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts.Models;

/// <summary>
/// Параметры запроса транзакций с пагинацией и фильтрацией
/// </summary>
public class TransactionQuery
{
	/// <summary>
	/// Номер страницы (начиная с 1)
	/// </summary>
	public int Page { get; set; } = 1;

	/// <summary>
	/// Размер страницы
	/// </summary>
	public int PageSize { get; set; } = 20;

	/// <summary>
	/// Фильтр по типу транзакции
	/// </summary>
	public TransactionType? Type { get; set; }

	/// <summary>
	/// Фильтр по идентификатору аккаунта (источник или назначение)
	/// </summary>
	public Guid? AccountId { get; set; }
}
