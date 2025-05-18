using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts
{
	/// <summary>
	/// Репозиторий для работы с <see cref="CashVault"/>
	/// </summary>
	public interface ICashVaultRepository : IBaseWriteRepository<CashVault>
	{
		/// <summary>
		/// Удаляет все денежные хранилища по идентификатору пользователя
		/// </summary>
		void DeleteCashVaultsByUserId(Guid userId);

		/// <summary>
		/// Возвращает денежное хранилище пользователя по идентификатору
		/// </summary>
		Task<CashVault?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает денежное хранилище по идентификатору
		/// </summary>
		Task<CashVault?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает денежное хранилище по названию и идентификатору пользователя
		/// </summary>
		Task<CashVault?> GetByNameAndUserIdAsync(Guid userId, string name, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает список денежных хранилищ по идентификатору пользователя
		/// </summary>
		Task<List<CashVault>> GetListByUserIdAsync(Guid userId, CancellationToken cancellationToken);
	}
}