using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts.Models;

/// <summary>
/// Транзакция с постингами (без полей мягкого удаления и даты создания постингов).
/// Проекция из репозитория для списков.
/// </summary>
public sealed class TransactionModel
{
	/// <summary>
	/// Идентификатор транзакции
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Тип транзакции
	/// </summary>
	public TransactionType Type { get; set; }

	/// <summary>
	/// Дата и время создания
	/// </summary>
	public DateTimeOffset MadeAt { get; set; }

	/// <summary>
	/// Проводки транзакции
	/// </summary>
	public List<PostingModel> Postings { get; set; } = [];
}
