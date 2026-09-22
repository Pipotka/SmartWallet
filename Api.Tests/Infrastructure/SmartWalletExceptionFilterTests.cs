using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Service.Exceptions;
using Services.Contracts.Models.Exceptions;
using Xunit;

namespace Nasurino.SmartWallet.Api.Tests.Infrastructure;

public class SmartWalletExceptionFilterTests
{
    [Fact]
    public void OnException_ShouldReturn500InternalError_ForUnhandledException()
    {
        var filter = new SmartWalletExceptionFilter();
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new InvalidOperationException("boom")
        };

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { Code = ErrorCodes.InternalError, Message = "Внутренняя ошибка сервера" });
    }

    [Fact]
    public void OnException_ShouldReturn400_ForSmartWalletValidationExceptionWithErrorCode()
    {
        var filter = new SmartWalletExceptionFilter();
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new SmartWalletValidationException(ErrorCodes.PostingsEmpty, "Список проводок пуст")
        };

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { Code = ErrorCodes.PostingsEmpty, Message = "Список проводок пуст" });
    }

    [Fact]
    public void OnException_ShouldReturn404_ForEntityNotFoundByIdTransactionEndpointWithAccountNotFound()
    {
        var filter = new SmartWalletExceptionFilter();
        var accountId = Guid.NewGuid();
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new EntityNotFoundByIdServiceException<Entities.TransactionEndpoint>(
                ErrorCodes.AccountNotFound, accountId, $"Счет {accountId} не найден")
        };

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { Code = ErrorCodes.AccountNotFound, Message = $"Счет {accountId} не найден" });
    }

    [Fact]
    public void OnException_ShouldReturn404_ForEntityNotFoundByIdTransactionEndpoint()
    {
        var filter = new SmartWalletExceptionFilter();
        var id = Guid.NewGuid();
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new EntityNotFoundByIdServiceException<Entities.TransactionEndpoint>(
                ErrorCodes.AccountNotFound, id, $"Счет {id} не найден")
        };

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { Code = ErrorCodes.AccountNotFound, Message = $"Счет {id} не найден" });
    }

    [Fact]
    public void OnException_ShouldReturn404_ForEntityNotFoundByIdTransaction()
    {
        var filter = new SmartWalletExceptionFilter();
        var id = Guid.NewGuid();
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new EntityNotFoundByIdServiceException<Entities.Transaction>(
                ErrorCodes.TransactionNotFound, id, $"Транзакция {id} не найдена")
        };

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { Code = ErrorCodes.TransactionNotFound, Message = $"Транзакция {id} не найдена" });
    }

    [Fact]
    public void OnException_ShouldReturn400_ForSmartWalletValidationException()
    {
        var filter = new SmartWalletExceptionFilter();
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new SmartWalletValidationException(
                new PropertyValidationError("Prop", "Error"))
        };

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void OnException_ShouldReturn401_ForAuthenticationServiceException()
    {
        var filter = new SmartWalletExceptionFilter();
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new AuthenticationServiceException("auth failed")
        };

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void OnException_ShouldReturn401_ForAuthorizationServiceException()
    {
        var filter = new SmartWalletExceptionFilter();
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new AuthorizationServiceException("forbidden")
        };

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void OnException_ShouldReturn403_ForEntityAccessServiceException()
    {
        var filter = new SmartWalletExceptionFilter();
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new EntityAccessServiceException("access denied")
        };

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }
}
