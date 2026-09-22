using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.BackgroundTaskSystem.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Services.AutoMappers;
using Nasurino.SmartWallet.UnitTests.Services.Infrastructure.Mock.Extensions;
using Services.Contracts;
using Xunit;

namespace Nasurino.SmartWallet.Services.Tests;

public class TransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISmartWalletValidateService> _validateServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ITransactionEndpointRepository> _transactionEndpointRepositoryMock;
    private readonly Mock<IPostingRepository> _postingRepositoryMock;
    private readonly Mock<IBackgroundTaskSystemProvider> _backgroundTaskSystemProviderMock;
    private readonly ITransactionService _transactionService;
    private readonly Dictionary<Guid, TransactionEndpoint> _endpoints = new();

    public TransactionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validateServiceMock = new Mock<ISmartWalletValidateService>();
        var mapper = new MapperConfiguration(conf => conf.AddProfile<ServiceModelMapper>()).CreateMapper();

        _userRepositoryMock = new Mock<IUserRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _transactionEndpointRepositoryMock = new Mock<ITransactionEndpointRepository>();
        _postingRepositoryMock = new Mock<IPostingRepository>();
        _backgroundTaskSystemProviderMock = new Mock<IBackgroundTaskSystemProvider>();

        _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TransactionRepository).Returns(_transactionRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TransactionEndpointRepository).Returns(_transactionEndpointRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.PostingRepository).Returns(_postingRepositoryMock.Object);

        var postingSettings = Microsoft.Extensions.Options.Options.Create(new PostingSettings
        {
            MaxPostingsPerTransaction = 100
        });

        _transactionService = new TransactionService(
            _unitOfWorkMock.Object,
            _validateServiceMock.Object,
            mapper,
            _backgroundTaskSystemProviderMock.Object,
            postingSettings);

        _transactionEndpointRepositoryMock
            .Setup(r => r.GetListByIdsAndUserIdAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid _, IReadOnlyCollection<Guid> ids, CancellationToken __) =>
                ids.Where(id => _endpoints.ContainsKey(id)).Select(id => _endpoints[id]).ToList());

        _transactionRepositoryMock
            .Setup(r => r.GetStorageBalancesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Guid> ids, CancellationToken _) => ids.ToDictionary(id => id, _ => 0m));

        _transactionRepositoryMock
            .Setup(r => r.GetCategoryBalancesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Guid> ids, CancellationToken _) => ids.ToDictionary(id => id, _ => 0m));
    }

    [Fact]
    public async Task CreateShouldCreateTransfer()
    {
        var userId = Guid.NewGuid();
        var source = Guid.NewGuid();
        var dest = Guid.NewGuid();
        AddEndpoint(source, userId, EndpointType.Storage);
        AddEndpoint(dest, userId, EndpointType.Storage);

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = source, Amount = -500m },
                new CreateTransactionPostingModel { AccountId = dest, Amount = 500m }
            ]
        };

        SetupUser(userId);

        var result = await _transactionService.CreateAsync(model, CancellationToken.None);

        result.Type.Should().Be(TransactionType.Transfer);
        result.Postings.Should().HaveCount(2);
        result.Postings.Should().ContainSingle(p => p.AccountId == source && p.Amount == -500m);
        result.Postings.Should().ContainSingle(p => p.AccountId == dest && p.Amount == 500m);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateShouldCreateExpense()
    {
        var userId = Guid.NewGuid();
        var storage = Guid.NewGuid();
        var cat1 = Guid.NewGuid();
        var cat2 = Guid.NewGuid();
        AddEndpoint(storage, userId, EndpointType.Storage);
        AddEndpoint(cat1, userId, EndpointType.Category);
        AddEndpoint(cat2, userId, EndpointType.Category);

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = storage, Amount = -300m },
                new CreateTransactionPostingModel { AccountId = cat1, Amount = 200m },
                new CreateTransactionPostingModel { AccountId = cat2, Amount = 100m }
            ]
        };

        SetupUser(userId);

        var result = await _transactionService.CreateAsync(model, CancellationToken.None);

        result.Type.Should().Be(TransactionType.Expense);
        result.Postings.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateShouldCreateAdjustmentIncreaseWithOnlyUserPostings()
    {
        var userId = Guid.NewGuid();
        var storage = Guid.NewGuid();
        AddEndpoint(storage, userId, EndpointType.Storage);

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = storage, Amount = 1000m }
            ]
        };

        SetupUser(userId);

        var result = await _transactionService.CreateAsync(model, CancellationToken.None);

        result.Type.Should().Be(TransactionType.AdjustmentIncrease);
        result.Postings.Should().ContainSingle(p => p.AccountId == storage && p.Amount == 1000m);
    }

    [Fact]
    public async Task CreateShouldCreateAdjustmentDecreaseAndAllowNegativeBalance()
    {
        var userId = Guid.NewGuid();
        var storage = Guid.NewGuid();
        AddEndpoint(storage, userId, EndpointType.Storage);

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = storage, Amount = -500m }
            ]
        };

        SetupUser(userId);

        var result = await _transactionService.CreateAsync(model, CancellationToken.None);

        result.Type.Should().Be(TransactionType.AdjustmentDecrease);
        result.Postings.Should().ContainSingle(p => p.AccountId == storage && p.Amount == -500m);
    }

    [Fact]
    public async Task CreateShouldReturnPostingsLimitExceeded()
    {
        var userId = Guid.NewGuid();
        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings = Enumerable.Range(0, 101)
                .Select(_ => new CreateTransactionPostingModel { AccountId = Guid.NewGuid(), Amount = 1m })
                .ToList()
        };

        SetupUser(userId);

        var act = () => _transactionService.CreateAsync(model, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<PostingsValidationException>();
        ex.Which.ErrorCode.Should().Be(ErrorCodes.PostingsLimitExceeded);
    }

    [Fact]
    public async Task CreateShouldReturnDuplicateAccountId()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = accountId, Amount = -100m },
                new CreateTransactionPostingModel { AccountId = accountId, Amount = 100m }
            ]
        };

        SetupUser(userId);

        var act = () => _transactionService.CreateAsync(model, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<PostingsValidationException>();
        ex.Which.ErrorCode.Should().Be(ErrorCodes.DuplicateAccountId);
    }

    [Fact]
    public async Task CreateShouldReturnInvalidPostingCombination()
    {
        var userId = Guid.NewGuid();
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        AddEndpoint(a, userId, EndpointType.Storage);
        AddEndpoint(b, userId, EndpointType.Storage);

        var model = new CreateTransactionModel
        {
            UserId = userId,
            Postings =
            [
                new CreateTransactionPostingModel { AccountId = a, Amount = 100m },
                new CreateTransactionPostingModel { AccountId = b, Amount = -50m }
            ]
        };

        SetupUser(userId);

        var act = () => _transactionService.CreateAsync(model, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<PostingsValidationException>();
        ex.Which.ErrorCode.Should().Be(ErrorCodes.InvalidPostingCombination);
    }

    private void AddEndpoint(Guid id, Guid userId, EndpointType type)
    {
        _endpoints[id] = new TransactionEndpoint
        {
            Id = id,
            UserId = userId,
            EndpointType = type,
            Value = 0m
        };

        _transactionEndpointRepositoryMock
            .Setup(r => r.GetByIdAndUserIdAsync(id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_endpoints[id]);
    }

    private void SetupUser(Guid userId)
    {
        _userRepositoryMock.GetUserByIdReturnNotNull(userId);
        _validateServiceMock
            .Setup(s => s.ValidateAsync(It.IsAny<CreateTransactionModel>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
