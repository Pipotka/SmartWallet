using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Service.Exceptions;
using Xunit;

namespace Nasurino.SmartWallet.Services.Tests;

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
            .Which.Value.Should().BeEquivalentTo(new { Code = "internal_error", Message = "Внутренняя ошибка сервера" });
    }

    [Fact]
    public void OnException_ShouldReturnCodedError_ForCodedServiceException()
    {
        var filter = new SmartWalletExceptionFilter();
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new CodedServiceException("POSTINGS_EMPTY", "Список проводок пуст", StatusCodes.Status400BadRequest)
        };

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
