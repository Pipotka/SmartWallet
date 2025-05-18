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
public class CashVaultController : Controller
{
	private readonly ICashVaultService cashVaultService;
	private readonly IIdentityProvider identityProvider;
	private readonly IMapper mapper;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="CashVaultController"/>
	/// </summary>
	public CashVaultController(ICashVaultService cashVaultService,
		IIdentityProvider identityProvider,
		IMapper mapper)
	{
		this.cashVaultService = cashVaultService;
		this.identityProvider = identityProvider;
		this.mapper = mapper;
	}

	/// <summary>
	/// Получает список денежных хранилищ по идентификатору пользователя
	/// </summary>
	[HttpGet("list")]
	[ProducesResponseType(typeof(ICollection<CashVaultApiModel>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetList(CancellationToken token)
	{
		var responce = await cashVaultService.GetListByUserIdAsync(identityProvider.Id, token);
		return Ok(mapper.Map<List<CashVaultApiModel>>(responce));
	}

	/// <summary>
	/// Создаёт новое денежное храшнилище
	/// </summary>
	[HttpPost]
	[ProducesResponseType(typeof(CashVaultApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Create([FromBody] CreateCashVaultApiModel request, CancellationToken token)
	{
		var model = mapper.Map<CreateCashVaultModel>(request);
		model.UserId = identityProvider.Id;
		var responce = await cashVaultService.CreateAsync(model, token);
		return Ok(mapper.Map<CashVaultApiModel>(responce));
	}

	/// <summary>
	/// Обновляет денежное храшнилище
	/// </summary>
	[HttpPut]
	[ProducesResponseType(typeof(CashVaultApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Update([FromBody] UpdateCashVaultApiModel request, CancellationToken token)
	{
		var model = mapper.Map<UpdateCashVaultModel>(request);
		model.UserId = identityProvider.Id;
		var responce = await cashVaultService.UpdateAsync(model, token);
		return Ok(mapper.Map<CashVaultApiModel>(responce));
	}

	/// <summary>
	/// Удаляет денежного храшнилища
	/// </summary>
	[HttpDelete]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Delete([FromBody] DeleteCashVaultApiModel request, CancellationToken token)
	{
		var model = mapper.Map<DeleteCashVaultModel>(request);
		await cashVaultService.DeleteAsync(identityProvider.Id, model, token);
		return Ok();
	}
}