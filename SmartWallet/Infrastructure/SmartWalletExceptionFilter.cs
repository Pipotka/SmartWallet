using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nasurino.SmartWallet.Service.Exceptions;

namespace Nasurino.SmartWallet.Infrastructure;

/// <summary>
/// Фильтр обработки исключительных событий
/// </summary>
public class SmartWalletExceptionFilter : IExceptionFilter
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
			case InvalidOperationSmartWalletEntityServiceException ex:
				SetExecptionContext(new BadRequestObjectResult(new ApiExceptionDetails() 
				{ 
					Message = ex.Message,
					StatusCode = 406
				})
				{
					StatusCode = StatusCodes.Status406NotAcceptable
				}, context);
				break;

			case AuthenticationServiceException ex:
				SetExecptionContext(new UnauthorizedObjectResult(new ApiExceptionDetails()
				{
					Message = ex.Message,
					StatusCode = 401
				})
				{
					StatusCode = StatusCodes.Status401Unauthorized
				}, context);
				break;

			case EntityNotFoundServiceException ex:
				SetExecptionContext(new BadRequestObjectResult(new ApiExceptionDetails()
				{
					Message = ex.Message,
					StatusCode = 404
				})
				{
					StatusCode = StatusCodes.Status404NotFound
				}, context);
				break;

			case SmartWalletValidationException ex:
				SetExecptionContext(new UnprocessableEntityObjectResult(new ApiExceptionDetails()
				{
					Message = ex.Message,
					StatusCode = 422
				})
				{
					StatusCode = StatusCodes.Status422UnprocessableEntity
				}, context);
				break;
				// [TODO] добавить фильтр для EntityAccessServiceException
		}

		return;

		static void SetExecptionContext(ObjectResult result, ExceptionContext context)
		{
			context.ExceptionHandled = true;
			var response = context.HttpContext.Response;
			response.StatusCode = result.StatusCode ?? StatusCodes.Status400BadRequest;
			context.Result = result;
		}
	}
}
