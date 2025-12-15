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
public sealed class UserController : Controller
{
	private readonly IUserService _userService;
	private readonly IIdentityProvider _identityProvider;
	private readonly IMapper _mapper;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="UserController"/>
	/// </summary>
	public UserController(IUserService userService,
		IIdentityProvider identityProvider,
		IMapper mapper)
	{
		_userService = userService;
		_identityProvider = identityProvider;
		_mapper = mapper;
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
		var responce = await _userService.GetUserByIdAsync(_identityProvider.Id, token);
		return Ok(_mapper.Map<UserApiModel>(responce));
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
		var responce = await _userService.RegistrationAsync(_mapper.Map<CreateUserModel>(request), token);
		return Ok(_mapper.Map<UserApiModel>(responce));
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
		var response = await _userService.LogInAsync(_mapper.Map<LogInModel>(request), token);
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
		var updateModel = _mapper.Map<UpdateUserModel>(request);
		updateModel.Id = _identityProvider.Id;
		var response = await _userService.UpdateAsync(updateModel, token);
		return Ok(_mapper.Map<UserApiModel>(response));
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
		var updateModel = _mapper.Map<DeleteUserModel>(request);
		updateModel.Id = _identityProvider.Id;
		await _userService.DeleteAsync(updateModel, token);
		return Ok();
	}
}