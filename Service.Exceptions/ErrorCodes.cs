namespace Nasurino.SmartWallet.Service.Exceptions;

/// <summary>
/// Централизованные коды ошибок сервиса
/// </summary>
public static class ErrorCodes
{
	/// <summary>Внутренняя ошибка сервера</summary>
	public const string InternalError = "internal_error";

	/// <summary>Сущность не найдена</summary>
	public const string NotFound = "not_found";

	/// <summary>Ошибка валидации</summary>
	public const string ValidationError = "validation_error";

	/// <summary>Неавторизованный доступ</summary>
	public const string Unauthorized = "unauthorized";

	/// <summary>Доступ запрещён</summary>
	public const string AccessDenied = "access_denied";

	/// <summary>Нарушение лимита</summary>
	public const string LimitViolation = "limit_violation";

	/// <summary>Список проводок пуст</summary>
	public const string PostingsEmpty = "postings_empty";

	/// <summary>Превышен лимит проводок</summary>
	public const string PostingsLimitExceeded = "postings_limit_exceeded";

	/// <summary>Некорректный идентификатор счёта</summary>
	public const string InvalidAccountId = "invalid_account_id";

	/// <summary>Нулевая сумма проводки</summary>
	public const string ZeroAmount = "zero_amount";

	/// <summary>Дублирующийся идентификатор счёта</summary>
	public const string DuplicateAccountId = "duplicate_account_id";

	/// <summary>Счёт не найден</summary>
	public const string AccountNotFound = "account_not_found";

	/// <summary>Транзакция не найдена</summary>
	public const string TransactionNotFound = "transaction_not_found";

	/// <summary>Недопустимая комбинация проводок</summary>
	public const string InvalidPostingCombination = "invalid_posting_combination";
}
