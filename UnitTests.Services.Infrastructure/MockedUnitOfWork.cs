using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;

namespace Nasurino.SmartWallet.UnitTests.Services.Infrastructure;

/// <summary>
/// Замоканый unit of work
/// </summary>
public class MockedUnitOfWork : IUnitOfWork
{
    private readonly Mock<ICashVaultRepository> mockedCashVaultRepository;
    private readonly Mock<ISpendingAreaRepository> mockedSpendingAreaRepository;
    private readonly Mock<ITransactionRepository> mockedTransactionRepository;
    private readonly Mock<IUserRepository> mockedUserRepository;

    ICashVaultRepository IUnitOfWork.CashVaultRepository => mockedCashVaultRepository.Object;

    ISpendingAreaRepository IUnitOfWork.SpendingAreaRepository => mockedSpendingAreaRepository.Object;

    ITransactionRepository IUnitOfWork.TransactionRepository => mockedTransactionRepository.Object;

    IUserRepository IUnitOfWork.UserRepository => mockedUserRepository.Object;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="MockedUnitOfWork"/>
    /// </summary>
    public MockedUnitOfWork(Mock<ICashVaultRepository>? mockedCashVaultRepository = null,
        Mock<ISpendingAreaRepository>? mockedSpendingAreaRepository = null,
        Mock<ITransactionRepository>? mockedTransactionRepository = null,
        Mock<IUserRepository>? mockedUserRepository = null)
    {
        this.mockedCashVaultRepository = mockedCashVaultRepository ?? new Mock<ICashVaultRepository>();
        this.mockedSpendingAreaRepository = mockedSpendingAreaRepository ?? new Mock<ISpendingAreaRepository>();
        this.mockedTransactionRepository =  mockedTransactionRepository ?? new Mock<ITransactionRepository>();
        this.mockedUserRepository = mockedUserRepository ?? new Mock<IUserRepository>();
    }

    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}