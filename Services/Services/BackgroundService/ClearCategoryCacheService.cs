using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Services.Contracts.BackgroundService;

namespace Nasurino.SmartWallet.Services.BackgroundJobs
{
    public class ClearCategoryCacheService(ITransactionEndpointRepository transactionEndpointRepository) : IClearCategoryCacheService
    {
        Task IClearCategoryCacheService.ClearCategoryCacheAsync()
            => transactionEndpointRepository.ClearCategoryValueCacheAsync(CancellationToken.None);
    }
}
