using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Models.Transaction;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Services.Contracts;

namespace Nasurino.SmartWallet.Controllers;

/// <summary>
/// Контроллер для работы с транзакциями
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TransactionController : Controller
{
	private readonly ITransactionService transactionService;
	private readonly IIdentityProvider identityProvider;
	private readonly IMapper mapper;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="TransactionController"/>
	/// </summary>
	public TransactionController(ITransactionService transactionService,
		IIdentityProvider identityProvider,
		IMapper mapper)
	{
		this.transactionService = transactionService;
		this.identityProvider = identityProvider;
		this.mapper = mapper;
	}

	/// <summary>
	/// Получает список транзакций по идентификатору пользователя
	/// </summary>
	[HttpGet("list")]
	[ProducesResponseType(typeof(ICollection<TransactionApiModel>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetList(CancellationToken token)
	{
		var responce = await transactionService.GetListByUserIdAsync(identityProvider.Id, token);
		return Ok(mapper.Map<List<TransactionApiModel>>(responce));
	}

	/// <summary>
	/// Создаёт новую транзакцию
	/// </summary>
	[HttpPost]
	[ProducesResponseType(typeof(TransactionApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Create([FromBody] CreateTransactionApiModel request, CancellationToken token)
	{
		var model = mapper.Map<CreateTransactionModel>(request);
		var response = await transactionService.CreateAsync(identityProvider.Id, model, token);
		return Ok(mapper.Map<TransactionApiModel>(response));
	}

	/// <summary>
	/// Удаляет транзакцию
	/// </summary>
	[HttpDelete]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Delete([FromBody] DeleteTransactionApiModel request, CancellationToken token)
	{
		var model = mapper.Map<DeleteTransactionModel>(request);
		await transactionService.DeleteAsync(identityProvider.Id, model, token);
		return Ok();
	}
}