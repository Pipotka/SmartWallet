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

    /// <summary>
    /// GetById должен вернуть null для несуществующего ID
    /// </summary>
    [Fact]
    async Task GetByIdShouldReturnNullForNonExistentId()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        var cashVaults = new List<CashVault>
        {
            _entityProvider.Create<CashVault>(),
            _entityProvider.Create<CashVault>()
        };

        await Context.AddRangeAsync(cashVaults);
        await Context.SaveChangesAsync();
        
        // Act
        var result = await _cashVaultRepository.GetByIdAsync(nonExistentId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// GetById не должен возвращать удаленные записи
    /// </summary>
    [Fact]
    async Task GetByIdShouldNotReturnDeletedRecords()
    {
        // Arrange
        var deletedId = Guid.NewGuid();
        
        var cashVaults = new List<CashVault>
        {
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = deletedId;
                x.DeletedAt = DateTime.UtcNow;
            }),
            _entityProvider.Create<CashVault>()
        };

        await Context.AddRangeAsync(cashVaults);
        await Context.SaveChangesAsync();
        
        // Act
        var result = await _cashVaultRepository.GetByIdAsync(deletedId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// GetByIdAndUserId должен вернуть значение
    /// </summary>
    [Fact]
    async Task GetByIdAndUserIdShouldReturnValue()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        
        var cashVaults = new List<CashVault>
        {
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = targetId;
                x.UserId = targetUserId;
            }),
            _entityProvider.Create<CashVault>(x => x.UserId = Guid.NewGuid()), // другой пользователь
            _entityProvider.Create<CashVault>(x => x.DeletedAt = DateTime.Now) // удаленный
        };

        await Context.AddRangeAsync(cashVaults);
        await Context.SaveChangesAsync();
        
        // Act
        var result = await _cashVaultRepository.GetByIdAndUserIdAsync(targetId, targetUserId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(targetId);
        result.UserId.Should().Be(targetUserId);
    }

    /// <summary>
    /// GetByIdAndUserId должен вернуть null при несовпадении UserId
    /// </summary>
    [Fact]
    async Task GetByIdAndUserIdShouldReturnNullWhenUserIdDoesNotMatch()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var existingUserId = Guid.NewGuid();
        var wrongUserId = Guid.NewGuid();
        
        var cashVaults = new List<CashVault>
        {
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = targetId;
                x.UserId = existingUserId;
            })
        };

        await Context.AddRangeAsync(cashVaults);
        await Context.SaveChangesAsync();
        
        // Act
        var result = await _cashVaultRepository.GetByIdAndUserIdAsync(targetId, wrongUserId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// GetByNameAndUserId должен вернуть значение
    /// </summary>
    [Fact]
    async Task GetByNameAndUserIdShouldReturnValue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var targetName = "Test Cash Vault";
        
        var cashVaults = new List<CashVault>
        {
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = userId;
                x.Name = targetName;
            }),
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = userId;
                x.Name = "Another Vault";
            }),
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = Guid.NewGuid(); // другой пользователь
                x.Name = targetName;
            })
        };

        await Context.AddRangeAsync(cashVaults);
        await Context.SaveChangesAsync();
        
        // Act
        var result = await _cashVaultRepository.GetByNameAndUserIdAsync(userId, targetName, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Name.Should().Be(targetName);
    }

    /// <summary>
    /// GetByNameAndUserId должен вернуть null для несуществующего имени
    /// </summary>
    [Fact]
    async Task GetByNameAndUserIdShouldReturnNullForNonExistentName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentName = "Non Existent Vault";
        
        var cashVaults = new List<CashVault>
        {
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = userId;
                x.Name = "Existing Vault";
            })
        };

        await Context.AddRangeAsync(cashVaults);
        await Context.SaveChangesAsync();
        
        // Act
        var result = await _cashVaultRepository.GetByNameAndUserIdAsync(userId, nonExistentName, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// GetByNameAndUserId должен быть регистронезависимым
    /// </summary>
    [Fact]
    async Task GetByNameAndUserIdShouldBeCaseInsensitive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var originalName = "Test Cash Vault";
        var searchName = "test cash vault"; // в другом регистре
        
        var cashVaults = new List<CashVault>
        {
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = userId;
                x.Name = originalName;
            })
        };

        await Context.AddRangeAsync(cashVaults);
        await Context.SaveChangesAsync();
        
        // Act
        var result = await _cashVaultRepository.GetByNameAndUserIdAsync(userId, searchName, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Name.Should().Be(originalName);
    }

    /// <summary>
    /// GetListByUserId должен вернуть список денежных хранилищ пользователя
    /// </summary>
    [Fact]
    async Task GetListByUserIdShouldReturnUserCashVaults()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        var cashVaults = new List<CashVault>
        {
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = targetUserId;
                x.Name = "Vault 1";
            }),
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = targetUserId;
                x.Name = "Vault 2";
            }),
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = otherUserId; // другой пользователь
                x.Name = "Other User Vault";
            }),
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = targetUserId;
                x.Name = "Vault 3";
                x.DeletedAt = DateTime.UtcNow; // удаленный
            })
        };

        await Context.AddRangeAsync(cashVaults);
        await Context.SaveChangesAsync();
        
        // Act
        var result = await _cashVaultRepository.GetListByUserIdAsync(targetUserId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.All(cv => cv.UserId == targetUserId).Should().BeTrue();
        result.All(cv => cv.DeletedAt == null).Should().BeTrue();
        result.Select(cv => cv.Name).Should().Contain(new[] { "Vault 1", "Vault 2" });
    }

    /// <summary>
    /// GetListByUserId должен вернуть пустой список для несуществующего пользователя
    /// </summary>
    [Fact]
    async Task GetListByUserIdShouldReturnEmptyListForNonExistentUser()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        
        var cashVaults = new List<CashVault>
        {
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = Guid.NewGuid();
                x.Name = "Some Vault";
            })
        };

        await Context.AddRangeAsync(cashVaults);
        await Context.SaveChangesAsync();
        
        // Act
        var result = await _cashVaultRepository.GetListByUserIdAsync(nonExistentUserId, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// DeleteCashVaultsByUserId должен удалить все денежные хранилища пользователя
    /// </summary>
    [Fact]
    async Task DeleteCashVaultsByUserIdShouldDeleteAllUserCashVaults()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        var cashVaults = new List<CashVault>
        {
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = targetUserId;
                x.Name = "Target Vault 1";
            }),
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = targetUserId;
                x.Name = "Target Vault 2";
            }),
            _entityProvider.Create<CashVault>(x => 
            {
                x.Id = Guid.NewGuid();
                x.UserId = otherUserId; // должен остаться
                x.Name = "Other User Vault";
            })
        };

        await Context.AddRangeAsync(cashVaults);
        await Context.SaveChangesAsync();
        
        // Act
        ((ICashVaultRepository)_cashVaultRepository).DeleteCashVaultsByUserId(targetUserId);
        await Context.SaveChangesAsync();
        
        // Assert
        var remainingVaults = await _cashVaultRepository.GetListByUserIdAsync(targetUserId, CancellationToken.None);
        remainingVaults.Should().BeEmpty();
        
        var otherUserVaults = await _cashVaultRepository.GetListByUserIdAsync(otherUserId, CancellationToken.None);
        otherUserVaults.Should().HaveCount(1);
        otherUserVaults.First().Name.Should().Be("Other User Vault");
    }

    /// <summary>
    /// Add должен добавить денежное хранилище
    /// </summary>
    [Fact]
    async Task AddShouldAddCashVault()
    {
        // Arrange
        var newCashVault = _entityProvider.Create<CashVault>(x => 
        {
            x.Id = Guid.NewGuid();
            x.UserId = Guid.NewGuid();
            x.Name = "New Cash Vault";
            x.Value = 100.0;
        });

        // Act
        _cashVaultRepository.Add(newCashVault);
        await Context.SaveChangesAsync();
        
        // Assert
        var savedCashVault = await _cashVaultRepository.GetByIdAsync(newCashVault.Id, CancellationToken.None);
        savedCashVault.Should().NotBeNull();
        savedCashVault.Name.Should().Be("New Cash Vault");
        savedCashVault.Value.Should().Be(100.0);
    }

    /// <summary>
    /// Update должен обновить денежное хранилище
    /// </summary>
    [Fact]
    async Task UpdateShouldUpdateCashVault()
    {
        // Arrange
        var cashVault = _entityProvider.Create<CashVault>(x => 
        {
            x.Id = Guid.NewGuid();
            x.UserId = Guid.NewGuid();
            x.Name = "Original Name";
            x.Value = 50.0;
        });

        await Context.AddAsync(cashVault);
        await Context.SaveChangesAsync();
        
        // изменяем данные
        cashVault.Name = "Updated Name";
        cashVault.Value = 200.0;

        // Act
        _cashVaultRepository.Update(cashVault);
        await Context.SaveChangesAsync();
        
        // Assert
        var updatedCashVault = await _cashVaultRepository.GetByIdAsync(cashVault.Id, CancellationToken.None);
        updatedCashVault.Should().NotBeNull();
        updatedCashVault.Name.Should().Be("Updated Name");
        updatedCashVault.Value.Should().Be(200.0);
    }

    /// <summary>
    /// Delete должен пометить денежное хранилище как удаленное
    /// </summary>
    [Fact]
    async Task DeleteShouldMarkCashVaultAsDeleted()
    {
        // Arrange
        var cashVault = _entityProvider.Create<CashVault>(x => 
        {
            x.Id = Guid.NewGuid();
            x.UserId = Guid.NewGuid();
            x.Name = "To Delete";
            x.Value = 100.0;
        });

        await Context.AddAsync(cashVault);
        await Context.SaveChangesAsync();
        
        // Act
        _cashVaultRepository.Delete(cashVault);
        await Context.SaveChangesAsync();
        
        // Assert
        var deletedCashVault = await _cashVaultRepository.GetByIdAsync(cashVault.Id, CancellationToken.None);
        deletedCashVault.Should().BeNull(); // не возвращается в обычных запросах
        
        // Проверяем в базе данных напрямую, что запись помечена как удаленная
        var allCashVaults = await Context.Set<CashVault>().ToListAsync();
        var foundCashVault = allCashVaults.FirstOrDefault(cv => cv.Id == cashVault.Id);
        foundCashVault.Should().NotBeNull();
        foundCashVault.DeletedAt.Should().NotBeNull();
    }
}