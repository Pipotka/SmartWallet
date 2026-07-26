using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Context.Repository;

/// <summary>
/// Репозиторий для <see cref="Posting"/>
/// </summary>
public sealed class PostingRepository(IDataStorageContext storage) : BaseWriteRepository<Posting>(storage), IPostingRepository
{
	/// <inheritdoc />
	public void AddRange(IEnumerable<Posting> postings)
	{
		foreach (var posting in postings)
		{
			Storage.Create(posting);
		}
	}

	/// <inheritdoc />
	public void UpdateRange(IEnumerable<Posting> postings)
	{
		foreach (var posting in postings)
		{
			Storage.Update(posting);
		}
	}
}
