using Nasurino.SmartWallet.Entities.Enums;

namespace Nasurino.SmartWallet.Services.Models.Models;

/// <summary>
/// Модель транзакции
/// </summary>
public class TransactionModel
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Тип транзакции
	/// </summary>
	public TransactionType Type { get; set; }

	/// <summary>
	/// Дата создания
	/// </summary>
	public DateTimeOffset MadeAt { get; set; }

	/// <summary>
	/// Проводки транзакции
	/// </summary>
	public List<PostingModel> Postings { get; set; } = [];
}
