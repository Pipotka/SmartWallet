using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository.Contracts;

/// <summary>
/// Репозиторий для работы с <see cref="Posting"/>
/// </summary>
public interface IPostingRepository : IBaseWriteRepository<Posting>
{
	/// <summary>
	/// Добавляет коллекцию постингов единой операцией
	/// </summary>
	/// <param name="postings">Коллекция постингов</param>
	void AddRange(IEnumerable<Posting> postings);
}
