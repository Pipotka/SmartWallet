using AutoMapper;
using FluentAssertions;
using Moq;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Options;
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
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly JwtOptions _jwtOptions;
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validateServiceMock = new Mock<ISmartWalletValidateService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _mapperMock = new Mock<IMapper>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _jwtOptions = new JwtOptions { Key = "test-key-for-unit-tests-min-16-chars", ExpiresMinutes = 15, RefreshExpiresDays = 7 };

        _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.RefreshTokenRepository).Returns(_refreshTokenRepositoryMock.Object);

        _userService = new UserService(
            _unitOfWorkMock.Object,
            _validateServiceMock.Object,
            _passwordHasherMock.Object,
            _jwtProviderMock.Object,
            _mapperMock.Object,
            _jwtOptions);
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

    /// <summary>
    /// LogInAsync Should Return AccessToken And RefreshToken When Credentials Are Valid
    /// </summary>
    [Fact]
    public async Task LogInAsync_ShouldReturnAccessTokenAndRefreshToken_WhenCredentialsAreValid()
    {
        // Arrange
        var model = new LogInModel { Email = "test@test.com", Password = "password123" };
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", HashedPassword = "hashed" };

        _validateServiceMock.Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(model.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify(model.Password, user.HashedPassword))
            .Returns(true);
        _mapperMock.Setup(m => m.Map<UserModel>(user)).Returns(new UserModel { Id = user.Id });
        _jwtProviderMock.Setup(j => j.GenerateToken(It.IsAny<UserModel>()))
            .Returns("access-token");

        // Act
        var result = await _userService.LogInAsync(model, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        _refreshTokenRepositoryMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// LogInAsync Should Throw AuthenticationServiceException When Password Is Wrong
    /// </summary>
    [Fact]
    public async Task LogInAsync_ShouldThrowAuthenticationServiceException_WhenPasswordIsWrong()
    {
        // Arrange
        var model = new LogInModel { Email = "test@test.com", Password = "wrong" };
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", HashedPassword = "hashed" };

        _validateServiceMock.Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(model.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify(model.Password, user.HashedPassword))
            .Returns(false);

        // Act
        var action = async () => await _userService.LogInAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<AuthenticationServiceException>();
    }

    /// <summary>
    /// RefreshAsync Should Return New Tokens When Refresh Token Is Valid
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "valid-refresh-token",
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };
        var user = new User { Id = userId };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("valid-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<UserModel>(user)).Returns(new UserModel { Id = userId });
        _jwtProviderMock.Setup(j => j.GenerateToken(It.IsAny<UserModel>()))
            .Returns("new-access-token");

        // Act
        var result = await _userService.RefreshAsync("valid-refresh-token", CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        _refreshTokenRepositoryMock.Verify(r => r.Update(It.IsAny<RefreshToken>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// RefreshAsync Should Throw AuthenticationServiceException When Refresh Token Not Found
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ShouldThrowAuthenticationServiceException_WhenRefreshTokenNotFound()
    {
        // Arrange
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("unknown-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var action = async () => await _userService.RefreshAsync("unknown-token", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<AuthenticationServiceException>();
    }

    /// <summary>
    /// RefreshAsync Should Throw AuthenticationServiceException When Refresh Token Is Expired
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ShouldThrowAuthenticationServiceException_WhenRefreshTokenIsExpired()
    {
        // Arrange
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "expired-token",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            RevokedAt = null
        };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("expired-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        var action = async () => await _userService.RefreshAsync("expired-token", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<AuthenticationServiceException>();
    }

    /// <summary>
    /// RefreshAsync Should Throw AuthenticationServiceException When Refresh Token Is Revoked
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ShouldThrowAuthenticationServiceException_WhenRefreshTokenIsRevoked()
    {
        // Arrange
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "revoked-token",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = DateTime.UtcNow.AddDays(-1)
        };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("revoked-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        var action = async () => await _userService.RefreshAsync("revoked-token", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<AuthenticationServiceException>();
    }

    /// <summary>
    /// LogoutAsync Should Revoke Refresh Token When Token Exists
    /// </summary>
    [Fact]
    public async Task LogoutAsync_ShouldRevokeRefreshToken_WhenTokenExists()
    {
        // Arrange
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "valid-token",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        await _userService.LogoutAsync("valid-token", CancellationToken.None);

        // Assert
        storedToken.RevokedAt.Should().NotBeNull();
        _refreshTokenRepositoryMock.Verify(r => r.Update(storedToken), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// LogoutAsync Should Do Nothing When Token Does Not Exist
    /// </summary>
    [Fact]
    public async Task LogoutAsync_ShouldDoNothing_WhenTokenDoesNotExist()
    {
        // Arrange
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("unknown-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        await _userService.LogoutAsync("unknown-token", CancellationToken.None);

        // Assert
        _refreshTokenRepositoryMock.Verify(r => r.Update(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// LogInAsync Should Throw EntityNotFoundServiceException When User Not Found By Email
    /// </summary>
    [Fact]
    public async Task LogInAsync_ShouldThrowEntityNotFoundServiceException_WhenUserNotFoundByEmail()
    {
        // Arrange
        var model = new LogInModel { Email = "nobody@test.com", Password = "pass" };

        _validateServiceMock.Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(model.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var action = async () => await _userService.LogInAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundServiceException>();
    }

    /// <summary>
    /// LogInAsync Should Throw SmartWalletValidationException When Validation Fails
    /// </summary>
    [Fact]
    public async Task LogInAsync_ShouldThrowSmartWalletValidationException_WhenValidationFails()
    {
        // Arrange
        var model = new LogInModel { Email = "test@test.com", Password = "pass" };
        var expectedValidationError = new PropertyValidationError(
            nameof(LogInModel),
            "Validation failed.");

        _validateServiceMock.Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .Throws(new SmartWalletValidationException(expectedValidationError));

        // Act
        var action = async () => await _userService.LogInAsync(model, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<SmartWalletValidationException>()
            .WithMessage("*Validation failed*");
    }

    /// <summary>
    /// RefreshAsync Should Throw AuthenticationServiceException When User Not Found
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ShouldThrowAuthenticationServiceException_WhenUserNotFound()
    {
        // Arrange
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "orphan-token",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync("orphan-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(storedToken.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var action = async () => await _userService.RefreshAsync("orphan-token", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<AuthenticationServiceException>();
    }
}
