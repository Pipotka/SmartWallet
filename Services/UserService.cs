using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Infrastructure;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Service.Infrastructure.Contracts;
using Services.Contracts;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис для работы с пользователем
/// </summary>
public sealed class UserService(IUnitOfWork unitOfWork,
	ISmartWalletValidateService validateService,
	IPasswordHasher passwordHasher,
	IJwtProvider jwtProvider,
	IMapper mapper,
	JwtOptions jwtOptions) : IUserService
{
	private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
	private readonly ITransactionEndpointRepository _transactionEndpointRepository = unitOfWork.TransactionEndpointRepository;
	private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;
	private readonly IRefreshTokenRepository _refreshTokenRepository = unitOfWork.RefreshTokenRepository;

	async Task<UserModel> IUserService.GetUserByIdAsync(Guid userId, CancellationToken token)
	{
		var user = await _userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundByIdServiceException<User>(userId);

		return mapper.Map<UserModel>(user);
	}

	async Task<UserModel> IUserService.RegistrationAsync(CreateUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		var user = mapper.Map<User>(model);
		user.Id = Guid.NewGuid();
		user.HashedPassword = passwordHasher.Generate(model.Password);
		_userRepository.Add(user);

		foreach (var spendingAreaName in new[] {
			"Продукты", "Кафе и рестораны","Транспорт",
			"Жилье", "Здоровье", "Одежда и обувь",
			"Развлечения", "Путешествия", "Образование",
			"Подарки"})
		{
			_transactionEndpointRepository.Add(
				new()
				{
					UserId = user.Id,
					Name = spendingAreaName,
					Value = 0.0m,
					IsStorage = false
				});
		}

		foreach (var cashVaultName in new[] { "Кошелёк", "Карта" })
		{
			_transactionEndpointRepository.Add(new()
			{
				UserId = user.Id,
				Name = cashVaultName,
				Value = 0.0m,
				IsStorage = true
			});
		}
		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<UserModel>(user);
	}

	async Task<(string AccessToken, string RefreshToken)> IUserService.LogInAsync(LogInModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		var user = await _userRepository.GetUserByEmailAsync(model.Email, token)
			?? throw new AuthenticationServiceException("Неверный логин или пароль");
		if (!passwordHasher.Verify(model.Password, user.HashedPassword))
		{
			throw new AuthenticationServiceException("Неверный логин или пароль");
		}

		var accessToken = jwtProvider.GenerateToken(mapper.Map<UserModel>(user));
		var refreshTokenValue = Guid.NewGuid().ToString();
		var refreshToken = new RefreshToken
		{
			Id = Guid.NewGuid(),
			Token = refreshTokenValue,
			UserId = user.Id,
			ExpiresAt = DateTimeOffset.UtcNow.AddDays(jwtOptions.RefreshExpiresDays),
			CreatedAt = DateTimeOffset.UtcNow,
		};
		_refreshTokenRepository.Add(refreshToken);
		await unitOfWork.SaveChangesAsync(token);

		return (accessToken, refreshTokenValue);
	}

	async Task<(string AccessToken, string RefreshToken)> IUserService.RefreshAsync(string refreshTokenValue, CancellationToken token)
	{
		var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, token)
			?? throw new AuthenticationServiceException();

		if (storedToken.ExpiresAt < DateTimeOffset.UtcNow)
		{
			throw new AuthenticationServiceException();
		}

		if (storedToken.RevokedAt is not null)
		{
			throw new AuthenticationServiceException();
		}

		var user = await _userRepository.GetUserByIdAsync(storedToken.UserId, token)
			?? throw new AuthenticationServiceException();

		// Mark old refresh token as revoked
		storedToken.RevokedAt = DateTimeOffset.UtcNow;

		// Generate new tokens
		var newAccessToken = jwtProvider.GenerateToken(mapper.Map<UserModel>(user));
		var newRefreshTokenValue = Guid.NewGuid().ToString();
		var newRefreshToken = new RefreshToken
		{
			Id = Guid.NewGuid(),
			Token = newRefreshTokenValue,
			UserId = user.Id,
			ExpiresAt = DateTimeOffset.UtcNow.AddDays(jwtOptions.RefreshExpiresDays),
			CreatedAt = DateTimeOffset.UtcNow,
		};

		storedToken.ReplacedByToken = newRefreshTokenValue;
		_refreshTokenRepository.Update(storedToken);
		_refreshTokenRepository.Add(newRefreshToken);
		await unitOfWork.SaveChangesAsync(token);

		return (newAccessToken, newRefreshTokenValue);
	}

	async Task IUserService.LogoutAsync(string refreshTokenValue, CancellationToken token)
	{
		var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, token);
		if (storedToken is not null)
		{
			storedToken.RevokedAt = DateTimeOffset.UtcNow;
			_refreshTokenRepository.Update(storedToken);
			await unitOfWork.SaveChangesAsync(token);
		}
	}

	async Task<UserModel> IUserService.UpdateAsync(UpdateUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);

		var user = await _userRepository.GetUserByIdAsync(model.Id, token)
			?? throw new EntityNotFoundByIdServiceException<User>(model.Id);
		mapper.Map(model, user);
		_userRepository.Update(user);
		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<UserModel>(user);
	}

	async Task IUserService.DeleteAsync(DeleteUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);

		var user = await _userRepository.GetUserByIdAsync(model.Id, token)
			?? throw new EntityNotFoundByIdServiceException<User>(model.Id);

		if (!passwordHasher.Verify(model.Password, user.HashedPassword))
		{
			throw new AuthenticationServiceException();
		}
		_userRepository.Delete(user);
		_transactionEndpointRepository.DeleteTransactionEndpointsByUserId(user.Id);
		_transactionRepository.DeleteTransactionsByUserId(user.Id);

		await unitOfWork.SaveChangesAsync(token);
	}

	async Task IUserService.ChangePasswordAsync(ChangePasswordModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);

		var user = await _userRepository.GetUserByIdAsync(model.UserId, token)
			?? throw new EntityNotFoundByIdServiceException<User>(model.UserId);

		if (!passwordHasher.Verify(model.OldPassword, user.HashedPassword))
		{
			throw new AuthenticationServiceException("Старый пароль указан неверно");
		}

		user.HashedPassword = passwordHasher.Generate(model.NewPassword);
		_userRepository.Update(user);
		await unitOfWork.SaveChangesAsync(token);
	}
}
