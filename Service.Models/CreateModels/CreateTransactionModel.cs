namespace Nasurino.SmartWallet.Service.Models.CreateModels;

/// <summary>
/// Модель создания транзакции
/// </summary>
public class CreateTransactionModel
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Список проводок транзакции
	/// </summary>
	public List<CreateTransactionPostingModel> Postings { get; set; } = [];
}
