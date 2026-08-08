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
		var exception = context.Exception as ServiceException;
		if (exception == null)
		{
			return;
		}

		switch (exception)
		{
			case AuthenticationServiceException ex:
				SetExceptionContext(new UnauthorizedObjectResult(new ApiExceptionDetails
				{
					Message = ex.Message,
					StatusCode = 401
				})
				{
					StatusCode = StatusCodes.Status401Unauthorized
				}, context);
				break;

			case AuthorizationServiceException ex:
				SetExceptionContext(new UnauthorizedObjectResult(new ApiExceptionDetails
				{
					Message = ex.Message,
					StatusCode = 401
				})
				{
					StatusCode = StatusCodes.Status401Unauthorized
				}, context);
				break;
			
			case EntityNotFoundServiceException ex:
				SetExceptionContext(new BadRequestObjectResult(new ApiExceptionDetails
				{
					Message = ex.Message,
					StatusCode = 404
				})
				{
					StatusCode = StatusCodes.Status404NotFound
				}, context);
				break;

			case SmartWalletValidationException ex:
				SetExceptionContext(new UnprocessableEntityObjectResult(new ApiExceptionDetails
				{
					Message = ex.Message,
					StatusCode = 422
				})
				{
					StatusCode = StatusCodes.Status422UnprocessableEntity
				}, context);
				break;
			
			case EntityAccessServiceException ex:
				SetExceptionContext(new BadRequestObjectResult(new ApiExceptionDetails
				{
					Message = ex.Message,
					StatusCode = 403
				})
				{
					StatusCode = StatusCodes.Status403Forbidden
				}, context);
				break;
			
			case AccountBalanceLimitViolationException ex:
				SetExceptionContext(new BadRequestObjectResult(new ApiExceptionDetails
				{
					Message = ex.Message,
					StatusCode = 409
				})
				{
					StatusCode = StatusCodes.Status409Conflict
				}, context);
				break;
		}

		return;

		static void SetExceptionContext(ObjectResult result, ExceptionContext context)
		{
			context.ExceptionHandled = true;
			var response = context.HttpContext.Response;
			response.StatusCode = result.StatusCode ?? StatusCodes.Status400BadRequest;
			context.Result = result;
		}
	}
}
