namespace Nasurino.SmartWallet.Service.Models;

/// <summary>
/// Обобщённый результат постраничного запроса (сервисный слой)
/// </summary>
public class PagedResultModel<T>
{
	/// <summary>
	/// Элементы текущей страницы
	/// </summary>
	public IReadOnlyList<T> Items { get; set; } = [];

	/// <summary>
	/// Общее количество элементов
	/// </summary>
	public int TotalCount { get; set; }

	/// <summary>
	/// Номер текущей страницы
	/// </summary>
	public int Page { get; set; }

	/// <summary>
	/// Размер страницы
	/// </summary>
	public int PageSize { get; set; }

	/// <summary>
	/// Общее количество страниц
	/// </summary>
	public int TotalPages { get; set; }
}
