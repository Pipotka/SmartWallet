using Ahatornn.TestGenerator;
using FluentAssertions;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Tests;
using Nasurino.SmartWallet.Entities;
using Xunit;

namespace Context.Repository.Tests;

/// <summary>
/// Тесты на <see cref="CashVaultRepository"/>
/// </summary>
public class CashVaultRepositoryTests : SmartWalletContextInMemory
{
    private readonly TestEntityProvider _entityProvider;
    private readonly ICashVaultRepository _cashVaultRepository;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="CashVaultRepositoryTests"/>
    /// </summary>
    public CashVaultRepositoryTests()
    {
        _entityProvider = TestEntityProvider.Shared;
        _cashVaultRepository = new CashVaultRepository(StorageContext);
    }
    
    /// <summary>
    /// GetById должен вернуть значение
    /// </summary>
    [Fact]
    async Task GetByIdShouldReturnValue()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        
        var cashVaults = new List<CashVault>
        {
            _entityProvider.Create<CashVault>(x => x.Id = targetId),
            _entityProvider.Create<CashVault>(x => x.DeletedAt = DateTime.Now),
            _entityProvider.Create<CashVault>()
        };

        await Context.AddRangeAsync(cashVaults);
        await Context.SaveChangesAsync();
        
        // Act
        var result = await _cashVaultRepository.GetByIdAsync(targetId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(targetId);
    }
}