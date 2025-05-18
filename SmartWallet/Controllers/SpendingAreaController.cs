using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Models.SpendingArea;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Services.Contracts;

namespace Nasurino.SmartWallet.Controllers;

/// <summary>
/// Контроллер для работы с областями трат
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SpendingAreaController : Controller
{
	private readonly ISpendingAreaService spendingAreaService;
	private readonly IIdentityProvider identityProvider;
	private readonly IMapper mapper;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="SpendingAreaController"/>
	/// </summary>
	public SpendingAreaController(ISpendingAreaService spendingAreaService,
		IIdentityProvider identityProvider,
		IMapper mapper)
	{
		this.spendingAreaService = spendingAreaService;
		this.identityProvider = identityProvider;
		this.mapper = mapper;
	}

	/// <summary>
	/// Получает список областей трат по идентификатору пользователя
	/// </summary>
	[HttpGet("list")]
	[ProducesResponseType(typeof(ICollection<SpendingAreaApiModel>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetList(CancellationToken token)
	{
		var responce = await spendingAreaService.GetListByUserIdAsync(identityProvider.Id, token);
		return Ok(mapper.Map<List<SpendingAreaApiModel>>(responce));
	}

	/// <summary>
	/// Создаёт новую область трат
	/// </summary>
	[HttpPost]
	[ProducesResponseType(typeof(SpendingAreaApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Create([FromBody] CreateSpendingAreaApiModel request, CancellationToken token)
	{
		var model = mapper.Map<CreateSpendingAreaModel>(request);
		model.UserId = identityProvider.Id;
		var responce = await spendingAreaService.CreateAsync(model, token);
		return Ok(mapper.Map<SpendingAreaApiModel>(responce));
	}

	/// <summary>
	/// Обновляет область трат
	/// </summary>
	[HttpPut]
	[ProducesResponseType(typeof(SpendingAreaApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Update([FromBody] UpdateSpendingAreaApiModel request, CancellationToken token)
	{
		var model = mapper.Map<UpdateSpendingAreaModel>(request);
		model.UserId = identityProvider.Id;
		var responce = await spendingAreaService.UpdateAsync(model, token);
		return Ok(mapper.Map<SpendingAreaApiModel>(responce));
	}

	/// <summary>
	/// Удаляет область трат
	/// </summary>
	[HttpDelete]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Delete([FromBody] DeleteSpendingAreaApiModel request, CancellationToken token)
	{
		var model = mapper.Map<DeleteSpendingAreaModel>(request);
		await spendingAreaService.DeleteAsync(identityProvider.Id, model, token);
		return Ok();
	}
}