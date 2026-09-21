using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Models;
using Nasurino.SmartWallet.Service.Exceptions;

namespace Nasurino.SmartWallet.Infrastructure;

/// <summary>
/// Глобальный фильтр исключений. Маппит типы исключений на HTTP-статусы,
/// коды ошибок берутся из <see cref="ServiceException.ErrorCode"/>.
/// </summary>
public sealed class SmartWalletExceptionFilter : IExceptionFilter
{
	public void OnException(ExceptionContext context)
	{
		switch (context.Exception)
		{
			case CodedServiceException coded:
				SetResult(context, MapCodedExceptionToStatusCode(coded), coded.ErrorCode, coded.Message);
				return;

			case EntityNotFoundByIdServiceException<TransactionEndpoint>:
				SetResult(context, StatusCodes.Status404NotFound, ErrorCodes.AccountNotFound, context.Exception.Message);
				return;

			case EntityNotFoundByIdServiceException<Transaction>:
				SetResult(context, StatusCodes.Status404NotFound, ErrorCodes.TransactionNotFound, context.Exception.Message);
				return;

			case EntityNotFoundServiceException ex:
				SetResult(context, StatusCodes.Status404NotFound, ex.ErrorCode, ex.Message);
				return;

			case SmartWalletValidationException ex:
				SetResult(context, StatusCodes.Status400BadRequest, ex.ErrorCode, ex.Message);
				return;

			case AuthenticationServiceException:
			case AuthorizationServiceException:
				SetResult(context, StatusCodes.Status401Unauthorized, ErrorCodes.Unauthorized, context.Exception.Message);
				return;

			case EntityAccessServiceException ex:
				SetResult(context, StatusCodes.Status403Forbidden, ex.ErrorCode, ex.Message);
				return;

			case AccountBalanceLimitViolationException ex:
				SetResult(context, StatusCodes.Status409Conflict, ex.ErrorCode, ex.Message);
				return;

			case ServiceException svc:
				SetResult(context, StatusCodes.Status500InternalServerError, svc.ErrorCode, svc.Message);
				return;
		}

		SetResult(context, StatusCodes.Status500InternalServerError, ErrorCodes.InternalError, "Внутренняя ошибка сервера");
	}

	private static int MapCodedExceptionToStatusCode(CodedServiceException ex) => ex.ErrorCode switch
	{
		ErrorCodes.PostingsEmpty => StatusCodes.Status400BadRequest,
		ErrorCodes.PostingsLimitExceeded => StatusCodes.Status400BadRequest,
		ErrorCodes.InvalidAccountId => StatusCodes.Status400BadRequest,
		ErrorCodes.ZeroAmount => StatusCodes.Status400BadRequest,
		ErrorCodes.DuplicateAccountId => StatusCodes.Status400BadRequest,
		ErrorCodes.InvalidPostingCombination => StatusCodes.Status400BadRequest,
		ErrorCodes.AccountNotFound => StatusCodes.Status404NotFound,
		ErrorCodes.SystemEndpointNotFound => StatusCodes.Status500InternalServerError,
		_ => StatusCodes.Status500InternalServerError
	};

	private static void SetResult(ExceptionContext context, int statusCode, string code, string message)
	{
		context.ExceptionHandled = true;
		context.HttpContext.Response.StatusCode = statusCode;
		context.Result = new ObjectResult(new ApiErrorDetails
		{
			Code = code,
			Message = message
		})
		{
			StatusCode = statusCode
		};
	}
}
