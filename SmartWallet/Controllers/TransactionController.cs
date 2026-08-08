using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Models;
using Nasurino.SmartWallet.Models.Transaction;
using Nasurino.SmartWallet.Services.Contracts;
using Nasurino.SmartWallet.Services.Models;
using Nasurino.SmartWallet.Services.Models.CreateModels;
using Nasurino.SmartWallet.Services.Models.DeleteModels;

namespace Nasurino.SmartWallet.Controllers;

/// <summary>
/// Контроллер для работы с транзакциями
/// </summary>
[Route("api/transactions")]
[ApiController]
[Authorize]
public sealed class TransactionController : Controller
{
	private readonly ITransactionService _transactionService;
	private readonly IIdentityProvider _identityProvider;
	private readonly IMapper _mapper;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="TransactionController"/>
	/// </summary>
	public TransactionController(ITransactionService transactionService,
		IIdentityProvider identityProvider,
		IMapper mapper)
	{
		_transactionService = transactionService;
		_identityProvider = identityProvider;
		_mapper = mapper;
	}

	/// <summary>
	/// Получает постраничный список транзакций пользователя с фильтрацией
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(PagedResultApiModel<TransactionApiModel>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetList([FromQuery] TransactionQueryApiModel request, CancellationToken token)
	{
		var query = _mapper.Map<TransactionQueryModel>(request);
		var response = await _transactionService.GetPagedListByUserIdAsync(_identityProvider.Id, query, token);
		return Ok(_mapper.Map<PagedResultApiModel<TransactionApiModel>>(response));
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
		var model = _mapper.Map<CreateTransactionModel>(request);
		model.UserId = _identityProvider.Id;
		var response = await _transactionService.CreateAsync(model, token);
		return Ok(_mapper.Map<TransactionApiModel>(response));
	}

	/// <summary>
	/// Удаляет транзакцию
	/// </summary>
	[HttpDelete]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Delete([FromBody] DeleteTransactionApiModel request, CancellationToken token)
	{
		var model = _mapper.Map<DeleteTransactionModel>(request);
		model.UserId = _identityProvider.Id;
		await _transactionService.DeleteAsync(model, token);
		return Ok();
	}
}