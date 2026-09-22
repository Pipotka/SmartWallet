using FluentAssertions;
using FluentAssertions.Specialized;
using Nasurino.SmartWallet.Services.Exceptions;

namespace Nasurino.SmartWallet.Services.UnitTests.Infrastructure.FluentAssertions.Shortcuts.Extensions;

/// <summary>
/// Методы расширения <see cref="Func{TResult}"/> для проверки на выбрасываемые исключения
/// </summary>
public static class ExceptionThrowingCheckExtensions
{
    public static Task<ExceptionAssertions<EntityNotFoundServiceException>> ShouldThrowEntityNotFoundException<TFuncResult>(this Func<Task<TFuncResult>> act,
        string because = "",
        params object[] becauseArgs)
        => act.Should().ThrowAsync<EntityNotFoundServiceException>(because, becauseArgs);
}