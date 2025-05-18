using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts
{
	/// <summary>
	/// Репозиторий для работы с <see cref="SpendingArea"/>
	/// </summary>
	public interface ISpendingAreaRepository : IBaseWriteRepository<SpendingArea>
	{
		/// <summary>
		/// Удаляет все области трат по идентификатору пользователя
		/// </summary>
		void DeleteSpendingAreasByUserId(Guid userId);

		/// <summary>
		/// Возвращает область трат пользователя по идентификатору
		/// </summary>
		Task<SpendingArea?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает область трат по идентификатору
		/// </summary>
		Task<SpendingArea?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает область трат по названию и идентификатору пользователя
		/// </summary>
		Task<SpendingArea?> GetByNameAndUserIdAsync(Guid userId, string name, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает список областей трат идентификатору пользователя
		/// </summary>
		Task<List<SpendingArea>> GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken);
	}
}