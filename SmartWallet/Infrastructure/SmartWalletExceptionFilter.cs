using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Models;
using Nasurino.SmartWallet.Service.Exceptions;

namespace Nasurino.SmartWallet.Infrastructure;

/// <summary>
/// Глобальный фильтр исключений. Маппит конкретные типы исключений на HTTP-статусы,
/// коды и сообщения берутся из свойств <see cref="ServiceException"/>.
/// </summary>
public sealed class SmartWalletExceptionFilter : IExceptionFilter
{
	public void OnException(ExceptionContext context)
	{
		switch (context.Exception)
		{
			case SmartWalletValidationException ex:
				SetResult(context, StatusCodes.Status400BadRequest, ex.ErrorCode, ex.Message);
				return;

			case EntityNotFoundByIdServiceException<TransactionEndpoint> ex:
				SetResult(context, StatusCodes.Status404NotFound, ex.ErrorCode, ex.Message);
				return;

			case EntityNotFoundByIdServiceException<Transaction> ex:
				SetResult(context, StatusCodes.Status404NotFound, ex.ErrorCode, ex.Message);
				return;

			case EntityNotFoundServiceException ex:
				SetResult(context, StatusCodes.Status404NotFound, ex.ErrorCode, ex.Message);
				return;

			case AuthenticationServiceException:
			case AuthorizationServiceException:
				SetResult(context, StatusCodes.Status401Unauthorized, ErrorCodes.Unauthorized, context.Exception.Message);
				return;

			case EntityAccessServiceException ex:
				SetResult(context, StatusCodes.Status403Forbidden, ex.ErrorCode, ex.Message);
				return;
		}

		SetResult(context, StatusCodes.Status500InternalServerError, ErrorCodes.InternalError, "Внутренняя ошибка сервера");
	}

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
