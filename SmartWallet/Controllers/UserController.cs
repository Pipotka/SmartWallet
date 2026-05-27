using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Models.Account;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Services.Contracts;

namespace Nasurino.SmartWallet.Controllers;

/// <summary>
/// Контроллер для работы с пользователем
/// </summary>
[Route("api/users")]
[ApiController]
public sealed class UserController : Controller
{
	private readonly IUserService _userService;
	private readonly IIdentityProvider _identityProvider;
	private readonly IWebHostEnvironment _environment;
	private readonly JwtOptions _jwtOptions;
	private readonly IMapper _mapper;

	/// <summary>
	/// Инициализирует новый экземпляр <see cref="UserController"/>
	/// </summary>
	public UserController(IUserService userService,
		IIdentityProvider identityProvider,
		IWebHostEnvironment environment,
		JwtOptions jwtOptions,
		IMapper mapper)
	{
		_userService = userService;
		_identityProvider = identityProvider;
		_environment = environment;
		_jwtOptions = jwtOptions;
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
		var response = await _userService.RegistrationAsync(_mapper.Map<CreateUserModel>(request), token);
		return Ok(_mapper.Map<UserApiModel>(response));
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
		var (accessToken, refreshToken) = await _userService.LogInAsync(_mapper.Map<LogInModel>(request), token);

		Response.Cookies.Append("refresh_token", refreshToken, CreateRefreshTokenCookieOptions(DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshExpiresDays)));
		return Ok(new ResponseLogInApiModel { AccessToken = accessToken });
	}

	/// <summary>
	/// Обновление access-токена
	/// </summary>
	[HttpPost("refresh")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(ResponseRefreshApiModel), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Refresh(CancellationToken token)
	{
		var refreshToken = Request.Cookies["refresh_token"];
		if (string.IsNullOrEmpty(refreshToken))
		{
			throw new AuthenticationServiceException();
		}

		try
		{
			var (accessToken, newRefreshToken) = await _userService.RefreshAsync(refreshToken, token);

			Response.Cookies.Append("refresh_token", newRefreshToken, CreateRefreshTokenCookieOptions(DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshExpiresDays)));
			return Ok(new ResponseRefreshApiModel { AccessToken = accessToken });
		}
		catch (AuthenticationServiceException)
		{
			DeleteRefreshTokenCookie();
			throw;
		}
	}

	/// <summary>
	/// Выход из аккаунта
	/// </summary>
	[HttpPost("logout")]
	[AllowAnonymous]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<IActionResult> LogOut(CancellationToken token)
	{
		var refreshToken = Request.Cookies["refresh_token"];
		if (!string.IsNullOrEmpty(refreshToken))
		{
			await _userService.LogoutAsync(refreshToken, token);
		}

		DeleteRefreshTokenCookie();
		return Ok();
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

	/// <summary>
	/// Смена пароля пользователя
	/// </summary>
	[HttpPut("password")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiExceptionDetails), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordApiModel request, CancellationToken token)
	{
		var model = _mapper.Map<ChangePasswordModel>(request);
		model.UserId = _identityProvider.Id;
		await _userService.ChangePasswordAsync(model, token);
		return Ok();
	}

	private void DeleteRefreshTokenCookie()
	{
		Response.Cookies.Append("refresh_token", "", CreateRefreshTokenCookieOptions(DateTimeOffset.UtcNow.AddDays(-1)));
	}

	private CookieOptions CreateRefreshTokenCookieOptions(DateTimeOffset expires)
	{
		return new CookieOptions
		{
			HttpOnly = true,
			Secure = _environment.IsProduction(),
			SameSite = SameSiteMode.Lax,
			Path = "/",
			Expires = expires
		};
	}
}
