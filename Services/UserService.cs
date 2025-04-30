using AutoMapper;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Infrastructure;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Nasurino.SmartWallet.Services.Validators;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис для работы с пользователем
/// </summary>
public class UserService(UnitOfWork unitOfWork,
	SmartWalletValidateService validateService,
	JwtProvider jwtProvider,
	IMapper mapper)
{
	private readonly UnitOfWork unitOfWork = unitOfWork;
	private readonly UserRepository userRepository = unitOfWork.UserRepository;
	private readonly CashVaultRepository cashVaultRepository = unitOfWork.CashVaultRepository;
	private readonly TransactionRepository transactionRepository = unitOfWork.TransactionRepository;
	private readonly SpendingAreaRepository spendingAreaRepository = unitOfWork.SpendingAreaRepository;
	private readonly SmartWalletValidateService validateService = validateService;
	private readonly JwtProvider jwtProvider = jwtProvider;
	private readonly IMapper mapper = mapper;

	/// <summary>
	/// Возвращает пользователя по Id
	/// </summary>
	public async Task<UserModel> GetUserByIdAsync(Guid userId, CancellationToken token)
	{
		var user = await userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с id = {userId} не найден.");
		
		return mapper.Map<UserModel>(user);
	}

	/// <summary>
	/// Регистрация
	/// </summary>
	public async Task<UserModel> RegistrationAsync(CreateUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		var user = mapper.Map<User>(model);
		user.Id = Guid.NewGuid();
		user.HashedPassword = PasswordHasher.Generate(model.Password);
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
		foreach (var cashVaultName in new[] {"Кошелёк", "Карта"})
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

	/// <summary>
	/// Вход в аккаунт
	/// </summary>
	public async Task<string> LogInAsync(LogInModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		var user = await userRepository.GetUserByEmailAsync(model.Email, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с адрессом электронной почты = {model.Email} не найден.");
		if (!PasswordHasher.Verify(model.Password, user.HashedPassword))
		{
			throw new AuthenticationServiceException("Аутентификация провалилась. Неверный логин или пароль.");
		}
		return jwtProvider.GenerateToken(mapper.Map<UserModel>(user));
	}

	/// <summary>
	/// Обновление пользователя
	/// </summary>
	public async Task<UserModel> UpdateAsync(UpdateUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);

		var user = await userRepository.GetUserByIdAsync(model.Id, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с Id = {model.Id} не найден.");
		mapper.Map(model, user);
		userRepository.Update(user);
		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<UserModel>(user);
	}

	/// <summary>
	/// Удаление пользователя
	/// </summary>
	public async Task DeleteAsync(DeleteUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);

		var user = await userRepository.GetUserByIdAsync(model.Id, token) 
			?? throw new EntityNotFoundServiceException($"Пользователь с Id = {model.Id} не найден.");

		if (!PasswordHasher.Verify(model.Password, user.HashedPassword))
		{
			throw new AuthenticationServiceException("Аутентификация провалилась. Неверный логин или пароль.");
		}
		userRepository.Delete(user);
		cashVaultRepository.DeleteCashVaultsByUserId(user.Id);
		spendingAreaRepository.DeleteSpendingAreasByUserId(user.Id);
		transactionRepository.DeleteTransactionsByUserId(user.Id);

		await unitOfWork.SaveChangesAsync(token);
	}
}