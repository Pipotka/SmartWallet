using Ahatornn.TestGenerator;
using FluentAssertions;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Tests;
using Nasurino.SmartWallet.Entities;
using Xunit;

namespace Nasurino.SmartWallet.Context.Repository.Tests;

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
        _entityProvider = new TestEntityProviderBuilder().Build();
        _transactionEndpointRepository = new TransactionEndpointRepository(StorageContext);
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

    /// <summary>
    /// GetListByUserId должен возвращать только не удаленные TransactionEndpoint
    /// </summary>
    [Fact]
    async Task GetListByUserIdShouldReturnOnlyNotDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        
        var transactionEndpoints = new List<TransactionEndpoint>
        {
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = userId;
                x.DeletedAt = DateTime.Now;
            }),
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = userId;
            }),
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = anotherUserId;
                x.DeletedAt = DateTime.Now;
            })
        };

        await Context.AddRangeAsync(transactionEndpoints);
        await Context.SaveChangesAsync();

        // Act
        var result = await _transactionEndpointRepository.GetListByUserIdAsync(userId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainSingle(x => x.UserId == userId && x.DeletedAt == null);
    }

    /// <summary>
    /// GetListByUserId должен возвращать только TransactionEndpoint пользователя
    /// </summary>
    [Fact]
    async Task GetListByUserIdShouldReturnOnlyUsersTransactionEndpoints()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        
        var transactionEndpoints = new List<TransactionEndpoint>
        {
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = userId;
            }),
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = anotherUserId;
            }),
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = userId;
            }),
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = anotherUserId;
                x.DeletedAt = DateTime.Now;
            })
        };

        await Context.AddRangeAsync(transactionEndpoints);
        await Context.SaveChangesAsync();

        // Act
        var result = await _transactionEndpointRepository.GetListByUserIdAsync(userId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.All(x => x.UserId == userId).Should().BeTrue();
    }

    /// <summary>
    /// GetListByUserId должен возвращать TransactionEndpoint в отсортированном виде (сначала IsStorage = true, потом IsStorage = false)
    /// </summary>
    [Fact]
    async Task GetListByUserIdShouldReturnInSortedOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        var transactionEndpoints = new List<TransactionEndpoint>
        {
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = userId;
                x.IsStorage = false;
            }),
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = userId;
                x.IsStorage = true;
            }),
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = userId;
                x.IsStorage = false;
            }),
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = userId;
                x.IsStorage = true;
            })
        };

        await Context.AddRangeAsync(transactionEndpoints);
        await Context.SaveChangesAsync();

        // Act
        var result = await _transactionEndpointRepository.GetListByUserIdAsync(userId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(4);
        
        // Проверяем, что сначала идут элементы с IsStorage = true
        for(int i = 0; i < result.Count - 1; i++)
        {
            if(result[i].IsStorage) 
            {
                // Если текущий элемент имеет IsStorage = true, то следующий может быть любым
                continue;
            }
            else
            {
                // Если текущий элемент имеет IsStorage = false, 
                // то все предыдущие должны были быть IsStorage = true
                for(int j = 0; j < i; j++)
                {
                    result[j].IsStorage.Should().BeTrue();
                }
            }
        }
        
        // Более точная проверка: все элементы с IsStorage=true должны идти перед элементами с IsStorage=false
        var firstFalseIndex = result.FindIndex(x => !x.IsStorage);
        if(firstFalseIndex != -1)
        {
            for(int i = 0; i < firstFalseIndex; i++)
            {
                result[i].IsStorage.Should().BeTrue("все элементы до первого с IsStorage=false должны иметь IsStorage=true");
            }
        }
    }

    /// <summary>
    /// GetListByUserId должен возвращать пустой список, если у пользователя нет TransactionEndpoint
    /// </summary>
    [Fact]
    async Task GetListByUserIdShouldReturnEmptyListWhenNoTransactionEndpointsForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        
        var transactionEndpoints = new List<TransactionEndpoint>
        {
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = anotherUserId;
            }),
            _entityProvider.Create<TransactionEndpoint>(x => {
                x.UserId = anotherUserId;
                x.DeletedAt = DateTime.Now;
            })
        };

        await Context.AddRangeAsync(transactionEndpoints);
        await Context.SaveChangesAsync();

        // Act
        var result = await _transactionEndpointRepository.GetListByUserIdAsync(userId, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}