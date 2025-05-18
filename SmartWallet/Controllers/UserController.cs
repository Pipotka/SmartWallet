using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Models.Account;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Services.Contracts;

namespace Nasurino.SmartWallet.Controllers;

/// <summary>
/// Контроллер для работы с пользователем
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UserController : Controller
{
	private readonly IUserService userService;
	private readonly IIdentityProvider identityProvider;
	private readonly IMapper mapper;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="UserController"/>
	/// </summary>
	public UserController(IUserService userService,
		IIdentityProvider identityProvider,
		IMapper mapper)
	{
		this.userService = userService;
		this.identityProvider = identityProvider;
		this.mapper = mapper;
	}

	/// <summary>
	/// Получает данные о пользователе
	/// </summary>
	[HttpGet]
	[Authorize]
	[ProducesResponseType(typeof(UserApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Get(CancellationToken token)
	{
		var responce = await userService.GetUserByIdAsync(identityProvider.Id, token);
		return Ok(mapper.Map<UserApiModel>(responce));
	}

	/// <summary>
	/// Регистрирует пользователя
	/// </summary>
	[HttpPost]
	[AllowAnonymous]
	[ProducesResponseType(typeof(UserApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	public async Task<IActionResult> SignIn([FromBody] CreateUserApiModel request, CancellationToken token)
	{
		var responce = await userService.RegistrationAsync(mapper.Map<CreateUserModel>(request), token);
		return Ok(mapper.Map<UserApiModel>(responce));
	}

	/// <summary>
	/// Вход в аккаунт
	/// </summary>
	[HttpPut("login")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(ResponseLogInApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> LogIn([FromBody] RequestLogInApiModel request, CancellationToken token)
	{
		var response = await userService.LogInAsync(mapper.Map<LogInModel>(request), token);
		return Ok(new ResponseLogInApiModel { JwtToken = response });
	}

	/// <summary>
	/// Обновление пользователя
	/// </summary>
	[HttpPut]
	[Authorize]
	[ProducesResponseType(typeof(UserApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Update([FromBody] UpdateUserApiModel request, CancellationToken token)
	{
		var updateModel = mapper.Map<UpdateUserModel>(request);
		updateModel.Id = identityProvider.Id;
		var responce = await userService.UpdateAsync(updateModel, token);
		return Ok(mapper.Map<UserApiModel>(responce));
	}

	/// <summary>
	/// Удаление пользователя
	/// </summary>
	[HttpDelete]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Delete([FromBody] DeleteUserApiModel request, CancellationToken token)
	{
		var updateModel = mapper.Map<DeleteUserModel>(request);
		updateModel.Id = identityProvider.Id;
		await userService.DeleteAsync(updateModel, token);
		return Ok();
	}
}