using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
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
public class UserService(IUnitOfWork unitOfWork,
	ISmartWalletValidateService validateService,
	IPasswordHasher passwordHasher,
	IJwtProvider jwtProvider,
	IMapper mapper) : IUserService
{
	private readonly IUnitOfWork unitOfWork = unitOfWork;
	private readonly IUserRepository userRepository = unitOfWork.UserRepository;
	private readonly ICashVaultRepository cashVaultRepository = unitOfWork.CashVaultRepository;
	private readonly ITransactionRepository transactionRepository = unitOfWork.TransactionRepository;
	private readonly ISpendingAreaRepository spendingAreaRepository = unitOfWork.SpendingAreaRepository;
	private readonly ISmartWalletValidateService validateService = validateService;
	private readonly IJwtProvider jwtProvider = jwtProvider;
	private readonly IPasswordHasher passwordHasher = passwordHasher;
	private readonly IMapper mapper = mapper;

	async Task<UserModel> IUserService.GetUserByIdAsync(Guid userId, CancellationToken token)
	{
		var user = await userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с id = {userId} не найден.");

		return mapper.Map<UserModel>(user);
	}

	async Task<UserModel> IUserService.RegistrationAsync(CreateUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		var user = mapper.Map<User>(model);
		user.Id = Guid.NewGuid();
		user.HashedPassword = passwordHasher.Generate(model.Password);
		userRepository.Add(user);

		//Создание базовых областей трат для пользователя
		foreach (var spendingAreaName in new[] {
			"Продукты", "Кафе и рестораны","Транспорт",
			"Жилье", "Здоровье", "Одежда и обувь",
			"Развлечения", "Путешествия", "Образование",
			"Подарки"})
		{
			spendingAreaRepository.Add(new()
			{
				Id = Guid.NewGuid(),
				UserId = user.Id,
				Name = spendingAreaName,
				Value = 0.0
			});
		}

		//Создание базовых денежных хранилищ для пользователя
		foreach (var cashVaultName in new[] { "Кошелёк", "Карта" })
		{
			cashVaultRepository.Add(new()
			{
				Id = Guid.NewGuid(),
				UserId = user.Id,
				Name = cashVaultName,
				Value = 0.0
			});
		}
		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<UserModel>(user);
	}

	async Task<string> IUserService.LogInAsync(LogInModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		var user = await userRepository.GetUserByEmailAsync(model.Email, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с адрессом электронной почты = {model.Email} не найден.");
		if (!passwordHasher.Verify(model.Password, user.HashedPassword))
		{
			throw new AuthorizationServiceException("Авторизация провалилась. Неверный логин или пароль.");
		}
		return jwtProvider.GenerateToken(mapper.Map<UserModel>(user));
	}

	async Task<UserModel> IUserService.UpdateAsync(UpdateUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);

		var user = await userRepository.GetUserByIdAsync(model.Id, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с Id = {model.Id} не найден.");
		mapper.Map(model, user);
		userRepository.Update(user);
		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<UserModel>(user);
	}

	async Task IUserService.DeleteAsync(DeleteUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);

		var user = await userRepository.GetUserByIdAsync(model.Id, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с Id = {model.Id} не найден.");

		if (!passwordHasher.Verify(model.Password, user.HashedPassword))
		{
			throw new AuthorizationServiceException("Аутентификация провалилась. Неверный логин или пароль.");
		}
		userRepository.Delete(user);
		cashVaultRepository.DeleteCashVaultsByUserId(user.Id);
		spendingAreaRepository.DeleteSpendingAreasByUserId(user.Id);
		transactionRepository.DeleteTransactionsByUserId(user.Id);

		await unitOfWork.SaveChangesAsync(token);
	}
}