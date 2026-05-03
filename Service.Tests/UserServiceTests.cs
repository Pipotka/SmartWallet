using AutoMapper;
using FluentAssertions;
using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Infrastructure;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Services.AutoMappers;
using Nasurino.SmartWallet.UnitTests.Services.Infrastructure.Mock.Extensions;
using Service.Infrastructure.Contracts;
using Services.Contracts;
using Services.Contracts.Models.Exceptions;
using Xunit;

namespace Nasurino.SmartWallet.Services.Tests;

/// <summary>
/// Тесты на UserService
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISmartWalletValidateService> _validateServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validateServiceMock = new Mock<ISmartWalletValidateService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _mapperMock = new Mock<IMapper>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_userRepositoryMock.Object);

        _userService = new UserService(
            _unitOfWorkMock.Object,
            _validateServiceMock.Object,
            _passwordHasherMock.Object,
            _jwtProviderMock.Object,
            _mapperMock.Object);
    }

    /// <summary>
    /// ChangePasswordAsync Should Change Password When Old Password Is Correct
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_ShouldChangePassword_WhenOldPasswordIsCorrect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var model = new ChangePasswordModel
        {
            UserId = userId,
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        _userRepositoryMock.GetUserByIdReturnNotNull(userId);

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _passwordHasherMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _passwordHasherMock.Setup(x => x.Generate(It.IsAny<string>()))
            .Returns("hashedPassword");

        // Act
        await _userService.ChangePasswordAsync(model, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ChangePasswordAsync Should Throw AuthenticationServiceException When Old Password Is Wrong
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_ShouldThrowAuthenticationServiceException_WhenOldPasswordIsWrong()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var model = new ChangePasswordModel
        {
            UserId = userId,
            OldPassword = "wrongOldPassword",
            NewPassword = "newPassword"
        };

        _userRepositoryMock.GetUserByIdReturnNotNull(userId);

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _passwordHasherMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        // Act
        var action = async () => await _userService.ChangePasswordAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<AuthenticationServiceException>();
    }

    /// <summary>
    /// ChangePasswordAsync Should Throw EntityNotFoundByIdServiceException When User Not Found
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_ShouldThrowEntityNotFoundByIdServiceException_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var model = new ChangePasswordModel
        {
            UserId = userId,
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var action = async () => await _userService.ChangePasswordAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundByIdServiceException<User>>();
    }

    /// <summary>
    /// ChangePasswordAsync Should Throw ValidationException When Validation Fails
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var model = new ChangePasswordModel
        {
            UserId = userId,
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        var expectedValidationError = new PropertyValidationError(
            nameof(ChangePasswordModel),
            "Validation failed.");

        _validateServiceMock.Setup(service => service.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Throws(new SmartWalletValidationException(expectedValidationError));

        // Act
        var action = async () => await _userService.ChangePasswordAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<SmartWalletValidationException>()
            .WithMessage("*Validation failed*");
    }
}