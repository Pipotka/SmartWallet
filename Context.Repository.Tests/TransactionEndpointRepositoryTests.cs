using Ahatornn.TestGenerator;
using FluentAssertions;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Tests;
using Nasurino.SmartWallet.Entities;
using Xunit;

namespace Context.Repository.Tests;

/// <summary>
/// Тесты на <see cref="Nasurino.SmartWallet.Context.Repository.TransactionEndpointRepository"/>
/// </summary>
public class TransactionEndpointRepositoryTests : SmartWalletContextInMemory
{
    private readonly TestEntityProvider _entityProvider;
    private readonly ITransactionEndpointRepository _transactionEndpointRepository;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="TransactionEndpointRepositoryTests"/>
    /// </summary>
    public TransactionEndpointRepositoryTests()
    {
        _entityProvider = TestEntityProvider.Shared;
        _transactionEndpointRepository = new Nasurino.SmartWallet.Context.Repository.TransactionEndpointRepository(StorageContext);
    }
    
    /// <summary>
    /// GetById должен вернуть значение
    /// </summary>
    [Fact]
    async Task GetByIdShouldReturnValue()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        
        var cashVaults = new List<TransactionEndpoint>
        {
            _entityProvider.Create<TransactionEndpoint>(x => x.Id = targetId),
            _entityProvider.Create<TransactionEndpoint>(x => x.DeletedAt = DateTime.Now),
            _entityProvider.Create<TransactionEndpoint>()
        };

        await Context.AddRangeAsync(cashVaults);
        await Context.SaveChangesAsync();
        
        // Act
        var result = await _transactionEndpointRepository.GetByIdAsync(targetId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(targetId);
    }
}