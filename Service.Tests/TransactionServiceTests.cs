using AutoMapper;
using FluentAssertions;
using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Services.AutoMappers;
using Nasurino.SmartWallet.UnitTests.Services.Infrastructure.Mock.Extensions;
using Services.Contracts;
using Services.Contracts.Models.Exceptions;
using Xunit;

namespace Nasurino.SmartWallet.Services.Tests;

/// <summary>
/// Тесты на TransactionService
/// </summary>
public class TransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISmartWalletValidateService> _validateServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ITransactionEndpointRepository> _transactionEndpointRepositoryMock;
    private readonly ITransactionService _transactionService;

    public TransactionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validateServiceMock = new Mock<ISmartWalletValidateService>();
        var mapper = new MapperConfiguration(conf => conf.AddProfile<ServiceModelMapper>()).CreateMapper();

        _userRepositoryMock = new Mock<IUserRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _transactionEndpointRepositoryMock = new Mock<ITransactionEndpointRepository>();

        _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TransactionRepository).Returns(_transactionRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TransactionEndpointRepository).Returns(_transactionEndpointRepositoryMock.Object);

        _transactionService = new TransactionService(
            _unitOfWorkMock.Object,
            _validateServiceMock.Object,
            mapper);
    }

    /// <summary>
    /// Create Should Throw Validation Error When Both Source And Destination Accounts Are Not Specified
    /// </summary>
    [Fact]
    public async Task CreateShouldThrowValidationErrorWhenBothAccountsAreNotSpecified()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var model = new CreateTransactionModel
        {
            UserId = userId,
            SourceAccountId = null,
            DestinationAccountId = null,
            Amount = 100.0
        };

        _userRepositoryMock.GetUserByIdReturnNotNull(userId);

        var expectedValidationError = new PropertyValidationError(
            nameof(CreateTransactionModel),
            $"По крайней мере одно из свойств ({nameof(CreateTransactionModel.SourceAccountId)} или {nameof(CreateTransactionModel.DestinationAccountId)}) должно иметь значение.");

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Throws(new SmartWalletValidationException(expectedValidationError));

        // Act
        var action = async () => await _transactionService.CreateAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<SmartWalletValidationException>()
            .WithMessage("*должно иметь значение*");
    }

    /// <summary>
    /// Create Should Create Income Transaction When Only Destination Account Is Specified And It's A Regular Account
    /// </summary>
    [Fact]
    public async Task CreateShouldCreateIncomeTransactionWhenOnlyDestinationAccountIsSpecifiedAndItsARegularAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();
        var model = new CreateTransactionModel
        {
            UserId = userId,
            SourceAccountId = null,
            DestinationAccountId = destinationAccountId,
            Amount = 100.0
        };

        var destinationAccount = new TransactionEndpoint
        {
            Id = destinationAccountId,
            IsStorage = true, // Это обычный счет/кошелек
            Value = 0.0
        };

        _userRepositoryMock.GetUserByIdReturnNotNull(userId);

        _transactionEndpointRepositoryMock.Setup(repo => repo.GetByIdAndUserIdAsync(destinationAccountId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount);

        _transactionRepositoryMock.Setup(repo => repo.GetBalanceByAccountIdAndDateRangeAsync(destinationAccountId, It.IsAny<CancellationToken>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(0.0);

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transactionService.CreateAsync(model, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DestinationAccountId.Should().Be(destinationAccountId);
        result.SourceAccountId.Should().BeNull();
        _transactionRepositoryMock.Verify(repo => repo.Add(It.IsAny<Transaction>()), Times.Once);
        _unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Create Should Throw Validation Error When Only Destination Account Is Specified And It's A Spending Area
    /// </summary>
    [Fact]
    public async Task CreateShouldThrowValidationErrorWhenOnlyDestinationAccountIsSpecifiedAndItsASpendingArea()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();
        var model = new CreateTransactionModel
        {
            UserId = userId,
            SourceAccountId = null,
            DestinationAccountId = destinationAccountId,
            Amount = 100.0
        };

        var destinationAccount = new TransactionEndpoint
        {
            Id = destinationAccountId,
            IsStorage = false, // Это область трат
            Value = 0.0
        };

        _userRepositoryMock.GetUserByIdReturnNotNull(userId);

        _transactionEndpointRepositoryMock.Setup(repo => repo.GetByIdAndUserIdAsync(destinationAccountId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount);

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var action = async () => await _transactionService.CreateAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<SmartWalletValidationException>()
            .WithMessage($"*{nameof(CreateTransactionModel.DestinationAccountId)} - Нельзя скорректировать баланс области трат*");
    }

    /// <summary>
    /// Create Should Throw Validation Error When Source Account Is A Spending Area
    /// </summary>
    [Fact]
    public async Task CreateShouldThrowValidationErrorWhenSourceAccountIsASpendingArea()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sourceAccountId = Guid.NewGuid();
        var model = new CreateTransactionModel
        {
            UserId = userId,
            SourceAccountId = sourceAccountId,
            DestinationAccountId = null,
            Amount = 100.0
        };

        var sourceAccount = new TransactionEndpoint
        {
            Id = sourceAccountId,
            IsStorage = false, // Это область трат
            Value = 0.0
        };

        _userRepositoryMock.GetUserByIdReturnNotNull(userId);

        _transactionEndpointRepositoryMock.Setup(repo => repo.GetByIdAndUserIdAsync(sourceAccountId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var action = async () => await _transactionService.CreateAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<SmartWalletValidationException>()
            .WithMessage($"*{nameof(CreateTransactionModel.SourceAccountId)} - Область трат не может быть указана как SourceAccount*");
    }

    /// <summary>
    /// Create Should Create Balance Decrease Adjustment When Only Source Account Is Specified
    /// </summary>
    [Fact]
    public async Task CreateShouldCreateBalanceDecreaseAdjustmentWhenOnlySourceAccountIsSpecified()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sourceAccountId = Guid.NewGuid();
        var model = new CreateTransactionModel
        {
            UserId = userId,
            SourceAccountId = sourceAccountId,
            DestinationAccountId = null,
            Amount = 100.0
        };

        var sourceAccount = new TransactionEndpoint
        {
            Id = sourceAccountId,
            IsStorage = true, // Это денежное хранилище
            Value = 1000.0
        };

        _userRepositoryMock.GetUserByIdReturnNotNull(userId);

        _transactionEndpointRepositoryMock.Setup(repo => repo.GetByIdAndUserIdAsync(sourceAccountId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);

        _transactionRepositoryMock.Setup(repo => repo.GetBalanceByAccountIdAndDateRangeAsync(sourceAccountId, It.IsAny<CancellationToken>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(1000.0);

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transactionService.CreateAsync(model, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SourceAccountId.Should().Be(sourceAccountId);
        result.DestinationAccountId.Should().BeNull();
        _transactionRepositoryMock.Verify(repo => repo.Add(It.IsAny<Transaction>()), Times.Once);
        _unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Create Should Create Transfer Transaction When Both Source And Destination Accounts Are Specified As Regular Accounts
    /// </summary>
    [Fact]
    public async Task CreateShouldCreateTransferTransactionWhenBothSourceAndDestinationAccountsAreSpecifiedAsRegularAccounts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sourceAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();
        var model = new CreateTransactionModel
        {
            UserId = userId,
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Amount = 100.0
        };

        var sourceAccount = new TransactionEndpoint
        {
            Id = sourceAccountId,
            IsStorage = true, // Это денежное хранилище
            Value = 1000.0
        };
        var destinationAccount = new TransactionEndpoint
        {
            Id = destinationAccountId,
            IsStorage = true, // Это обычный счет
            Value = 0.0
        };

        _userRepositoryMock.GetUserByIdReturnNotNull(userId);

        _transactionEndpointRepositoryMock.Setup(repo => repo.GetByIdAndUserIdAsync(sourceAccountId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);

        _transactionEndpointRepositoryMock.Setup(repo => repo.GetByIdAndUserIdAsync(destinationAccountId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount);

        _transactionRepositoryMock.Setup(repo => repo.GetBalanceByAccountIdAndDateRangeAsync(sourceAccountId, It.IsAny<CancellationToken>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(1000.0);

        _transactionRepositoryMock.Setup(repo => repo.GetBalanceByAccountIdAndDateRangeAsync(destinationAccountId, It.IsAny<CancellationToken>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(0.0);

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transactionService.CreateAsync(model, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SourceAccountId.Should().Be(sourceAccountId);
        result.DestinationAccountId.Should().Be(destinationAccountId);
        _transactionRepositoryMock.Verify(repo => repo.Add(It.IsAny<Transaction>()), Times.Once);
        _unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Create Should Create Expense Transaction When Source Is Storage And Destination Is Spending Area
    /// </summary>
    [Fact]
    public async Task CreateShouldCreateExpenseTransactionWhenSourceIsStorageAndDestinationIsSpendingArea()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sourceAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();
        var model = new CreateTransactionModel
        {
            UserId = userId,
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Amount = 100.0
        };

        var sourceAccount = new TransactionEndpoint
        {
            Id = sourceAccountId,
            IsStorage = true, // Это денежное хранилище
            Value = 1000.0
        };
        var destinationAccount = new TransactionEndpoint
        {
            Id = destinationAccountId,
            IsStorage = false, // Это область трат
            Value = 0.0
        };

        _userRepositoryMock.GetUserByIdReturnNotNull(userId);

        _transactionEndpointRepositoryMock.Setup(repo => repo.GetByIdAndUserIdAsync(sourceAccountId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);

        _transactionEndpointRepositoryMock.Setup(repo => repo.GetByIdAndUserIdAsync(destinationAccountId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount);

        _transactionRepositoryMock.Setup(repo => repo.GetBalanceByAccountIdAndDateRangeAsync(sourceAccountId, It.IsAny<CancellationToken>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(1000.0);

        _transactionRepositoryMock.Setup(repo => repo.GetBalanceByAccountIdAndDateRangeAsync(destinationAccountId, It.IsAny<CancellationToken>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(0.0);

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transactionService.CreateAsync(model, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SourceAccountId.Should().Be(sourceAccountId);
        result.DestinationAccountId.Should().Be(destinationAccountId);
        _transactionRepositoryMock.Verify(repo => repo.Add(It.IsAny<Transaction>()), Times.Once);
        _unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}