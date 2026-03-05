using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Models.CashVault;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Services.Contracts;

namespace Nasurino.SmartWallet.Controllers;

/// <summary>
/// Контроллер для работы с денежными хранилищами
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class TransactionEndpointController : Controller
{
	private readonly ITransactionEndpointService _transactionEndpointService;
	private readonly IIdentityProvider _identityProvider;
	private readonly IMapper _mapper;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="TransactionEndpointController"/>
	/// </summary>
	public TransactionEndpointController(ITransactionEndpointService transactionEndpointService,
		IIdentityProvider identityProvider,
		IMapper mapper)
	{
		_transactionEndpointService = transactionEndpointService;
		_identityProvider = identityProvider;
		_mapper = mapper;
	}

	/// <summary>
	/// Получает список денежных хранилищ по идентификатору пользователя
	/// </summary>
	[HttpGet("list")]
	[ProducesResponseType(typeof(ICollection<TransactionEndpointApiModel>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetList(CancellationToken token)
	{
		var response = await _transactionEndpointService.GetListByUserIdAsync(_identityProvider.Id, token);
		return Ok(_mapper.Map<List<TransactionEndpointApiModel>>(response));
	}

	/// <summary>
	/// Создаёт новое денежное храшнилище
	/// </summary>
	[HttpPost]
	[ProducesResponseType(typeof(TransactionEndpointApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Create([FromBody] CreateTransactionEndpointApiModel request, CancellationToken token)
	{
		var model = _mapper.Map<CreateTransactionEndpointModel>(request);
		model.UserId = _identityProvider.Id;
		var response = await _transactionEndpointService.CreateAsync(model, token);
		return Ok(_mapper.Map<TransactionEndpointApiModel>(response));
	}

	/// <summary>
	/// Обновляет денежное храшнилище
	/// </summary>
	[HttpPut]
	[ProducesResponseType(typeof(TransactionEndpointApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Update([FromBody] UpdateTransactionEndpointApiModel request, CancellationToken token)
	{
		var model = _mapper.Map<UpdateTransactionEndpointModel>(request);
		model.UserId = _identityProvider.Id;
		var response = await _transactionEndpointService.UpdateAsync(model, token);
		return Ok(_mapper.Map<TransactionEndpointApiModel>(response));
	}

	/// <summary>
	/// Удаляет денежного храшнилища
	/// </summary>
	[HttpDelete]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Delete([FromBody] DeleteTransactionEndpointApiModel request, CancellationToken token)
	{
		var model = _mapper.Map<DeleteTransactionEndpointModel>(request);
		model.UserId = _identityProvider.Id;
		await _transactionEndpointService.DeleteAsync(model, token);
		return Ok();
	}
}