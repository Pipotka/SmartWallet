using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nasurino.SmartWallet.Services.Exceptions;

namespace Nasurino.SmartWallet.Infrastructure;

/// <summary>
/// Фильтр обработки исключительных событий
/// </summary>
public sealed class SmartWalletExceptionFilter : IExceptionFilter
{
    /// <summary>
    /// <inheritdoc cref="IExceptionFilter.OnException(ExceptionContext)" path="/summary"/>
    /// </summary>
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ServiceException serviceException)
        {
            SetExceptionContext(
                StatusCodes.Status500InternalServerError,
                ErrorCodes.InternalError,
                "Внутренняя ошибка сервера",
                context);
            return;
        }

        switch (serviceException)
        {
            case AuthenticationServiceException ex:
                SetExceptionContext(
                    StatusCodes.Status401Unauthorized,
                    ex.ErrorCode,
                    ex.Message,
                    context);
                break;

            case AuthorizationServiceException ex:
                SetExceptionContext(
                    StatusCodes.Status401Unauthorized,
                    ex.ErrorCode,
                    ex.Message,
                    context);
                break;

            case EntityNotFoundServiceException ex:
                SetExceptionContext(
                    StatusCodes.Status404NotFound,
                    ex.ErrorCode,
                    ex.Message,
                    context);
                break;

            case SmartWalletValidationException ex:
                SetExceptionContext(
                    StatusCodes.Status400BadRequest,
                    ex.ErrorCode,
                    ex.Message,
                    context);
                break;

            case EntityAccessServiceException ex:
                SetExceptionContext(
                    StatusCodes.Status403Forbidden,
                    ex.ErrorCode,
                    ex.Message,
                    context);
                break;

            case AccountBalanceLimitViolationException ex:
                SetExceptionContext(
                    StatusCodes.Status409Conflict,
                    ex.ErrorCode,
                    ex.Message,
                    context);
                break;

            default:
                SetExceptionContext(
                    StatusCodes.Status500InternalServerError,
                    serviceException.ErrorCode,
                    serviceException.Message,
                    context);
                break;
        }
    }

    private static void SetExceptionContext(
        int statusCode,
        string code,
        string message,
        ExceptionContext context)
    {
        context.ExceptionHandled = true;
        context.HttpContext.Response.StatusCode = statusCode;
        context.Result = new ObjectResult(new ApiExceptionDetails
        {
            Code = code,
            StatusCode = statusCode,
            Message = message
        })
        {
            StatusCode = statusCode
        };
    }
}
