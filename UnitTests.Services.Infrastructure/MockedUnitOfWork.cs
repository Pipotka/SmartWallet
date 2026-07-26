using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;

namespace Nasurino.SmartWallet.UnitTests.Services.Infrastructure;

/// <summary>
/// Замоканый unit of work
/// </summary>
public sealed class MockedUnitOfWork : IUnitOfWork
{
    private readonly Mock<ITransactionEndpointRepository> _mockedCashVaultRepository;
    private readonly Mock<ITransactionRepository> _mockedTransactionRepository;
    private readonly Mock<IDailyExpenseCategorieRepository> _mockedDailyExpenseCategorieRepository;
    private readonly Mock<IUserRepository> _mockedUserRepository;
    private readonly Mock<IRefreshTokenRepository> _mockedRefreshTokenRepository;
    private readonly Mock<IPostingRepository> _mockedPostingRepository;

    ITransactionEndpointRepository IUnitOfWork.TransactionEndpointRepository => _mockedCashVaultRepository.Object;

    ITransactionRepository IUnitOfWork.TransactionRepository => _mockedTransactionRepository.Object;

    IDailyExpenseCategorieRepository IUnitOfWork.DailyExpenseCategorieRepository => _mockedDailyExpenseCategorieRepository.Object;

    IUserRepository IUnitOfWork.UserRepository => _mockedUserRepository.Object;

    IRefreshTokenRepository IUnitOfWork.RefreshTokenRepository => _mockedRefreshTokenRepository.Object;

    IPostingRepository IUnitOfWork.PostingRepository => _mockedPostingRepository.Object;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="MockedUnitOfWork"/>
    /// </summary>
    public MockedUnitOfWork(Mock<ITransactionEndpointRepository>? mockedCashVaultRepository = null,
        Mock<ITransactionRepository>? mockedTransactionRepository = null,
        Mock<IDailyExpenseCategorieRepository>? mockedDailyExpenseCategorieRepository = null,
        Mock<IUserRepository>? mockedUserRepository = null,
        Mock<IRefreshTokenRepository>? mockedRefreshTokenRepository = null)
    {
        _mockedCashVaultRepository = mockedCashVaultRepository ?? new Mock<ITransactionEndpointRepository>();
        _mockedTransactionRepository = mockedTransactionRepository ?? new Mock<ITransactionRepository>();
        _mockedDailyExpenseCategorieRepository = mockedDailyExpenseCategorieRepository ?? new Mock<IDailyExpenseCategorieRepository>();
        _mockedUserRepository = mockedUserRepository ?? new Mock<IUserRepository>();
        _mockedRefreshTokenRepository = mockedRefreshTokenRepository ?? new Mock<IRefreshTokenRepository>();
        _mockedPostingRepository = new Mock<IPostingRepository>();
    }

    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
