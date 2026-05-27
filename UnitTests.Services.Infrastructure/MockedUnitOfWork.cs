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
    private readonly Mock<IUserRepository> _mockedUserRepository;
    private readonly Mock<IRefreshTokenRepository> _mockedRefreshTokenRepository;

    ITransactionEndpointRepository IUnitOfWork.TransactionEndpointRepository => _mockedCashVaultRepository.Object;

    ITransactionRepository IUnitOfWork.TransactionRepository => _mockedTransactionRepository.Object;

    IUserRepository IUnitOfWork.UserRepository => _mockedUserRepository.Object;

    IRefreshTokenRepository IUnitOfWork.RefreshTokenRepository => _mockedRefreshTokenRepository.Object;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="MockedUnitOfWork"/>
    /// </summary>
    public MockedUnitOfWork(Mock<ITransactionEndpointRepository>? mockedCashVaultRepository = null,
        Mock<ITransactionRepository>? mockedTransactionRepository = null,
        Mock<IUserRepository>? mockedUserRepository = null,
        Mock<IRefreshTokenRepository>? mockedRefreshTokenRepository = null)
    {
        _mockedCashVaultRepository = mockedCashVaultRepository ?? new Mock<ITransactionEndpointRepository>();
        _mockedTransactionRepository = mockedTransactionRepository ?? new Mock<ITransactionRepository>();
        _mockedUserRepository = mockedUserRepository ?? new Mock<IUserRepository>();
        _mockedRefreshTokenRepository = mockedRefreshTokenRepository ?? new Mock<IRefreshTokenRepository>();
    }

    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
