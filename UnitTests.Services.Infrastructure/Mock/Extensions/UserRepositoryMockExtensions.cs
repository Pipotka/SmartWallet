using Moq;
using Moq.Language.Flow;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.UnitTests.Services.Infrastructure.Mock.Extensions;

/// <summary>
/// Методы-расширения для моков <see cref="IUserRepository"/>
/// </summary>
public static class UserRepositoryMockExtensions
{
    /// <summary>
    /// Метод <c>GetUserByIdAsync</c> вернёт не <c>null</c>
    /// </summary>
    /// <param name="userId">Идентификатор пользователя, с которым должен вызываться метод</param>
    /// <remarks>Если параметры не указаны - метод будет возвращать не <c>null</c> при вызове с любыми параметрами</remarks>
    public static void GetUserByIdReturnNotNull(this Mock<IUserRepository> mockedUserRepository,
        Guid userId = default,
        CancellationToken token = default)
        => mockedUserRepository.SetupGetUserById(userId, token)
            .ReturnsAsync(new User{ Id = userId });

    private static ISetup<IUserRepository, Task<User?>> SetupGetUserById(
        this Mock<IUserRepository> mockedUserRepository,
        Guid userId = default,
        CancellationToken token = default)
    {
        return mockedUserRepository.Setup(x => x.GetUserByIdAsync(
            It.Is<Guid>(g => userId == Guid.Empty || g == userId),
            It.Is<CancellationToken>(ct => token == CancellationToken.None || ct == token)
        ));
    }
}