using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Models;
using Nasurino.SmartWallet.Service.Exceptions;

namespace Nasurino.SmartWallet.Infrastructure;

public sealed class SmartWalletExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case CodedServiceException coded:
                SetResult(context, coded.StatusCode, coded.ErrorCode, coded.Message);
                return;

            case EntityNotFoundByIdServiceException<TransactionEndpoint> ex:
                SetResult(context, StatusCodes.Status404NotFound, "ACCOUNT_NOT_FOUND", ex.Message);
                return;

            case EntityNotFoundByIdServiceException<Transaction> ex:
                SetResult(context, StatusCodes.Status404NotFound, "TRANSACTION_NOT_FOUND", ex.Message);
                return;

            case EntityNotFoundServiceException ex:
                SetResult(context, StatusCodes.Status404NotFound, "not_found", ex.Message);
                return;

            case SmartWalletValidationException ex:
                SetResult(context, StatusCodes.Status400BadRequest, "VALIDATION_ERROR", ex.Message);
                return;

            case AuthenticationServiceException:
            case AuthorizationServiceException:
                SetResult(context, StatusCodes.Status401Unauthorized, "unauthorized", context.Exception.Message);
                return;

            case EntityAccessServiceException ex:
                SetResult(context, StatusCodes.Status403Forbidden, "access_denied", ex.Message);
                return;

            case AccountBalanceLimitViolationException ex:
                SetResult(context, StatusCodes.Status409Conflict, "limit_violation", ex.Message);
                return;
        }

        SetResult(context, StatusCodes.Status500InternalServerError, "internal_error", "Внутренняя ошибка сервера");
    }

    private static void SetResult(ExceptionContext context, int statusCode, string code, string message)
    {
        context.ExceptionHandled = true;
        context.HttpContext.Response.StatusCode = statusCode;
        context.Result = new ObjectResult(new ApiErrorApiModel
        {
            Code = code,
            Message = message
        })
        {
            StatusCode = statusCode
        };
    }
}
