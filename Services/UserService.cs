using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Infrastructure;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Services.Validators;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис для работы с пользователем
/// </summary>
public class UserService(UserRepository userRepository, ISmartWalletValidateService validateService)
{
	private readonly UserRepository userRepository = userRepository;
	private readonly ISmartWalletValidateService validateService = validateService;


	/// <summary>
	/// Регистрация
	/// </summary>
	public async Task Registration(CreateUserModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		// Создание User модели для нового пользователя при помощи mapper
		PasswordHasher.Generate(model.Password);
		// Присвоение Хэша пользователю

		//Создание базовых областей трат для пользователя

		//Создание базовых денежных хранилищ для пользователя

	}

	/// <summary>
	/// Вход в аккаунт
	/// </summary>
	public async Task LogIn(LogInModel model, CancellationToken token)
	{
		var user = await userRepository.GetUserByEmail(model.Email, token);
		if (user == null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с адрессом электронной почты = {model.Email} не найден.");
		}
		if (!PasswordHasher.Verify(model.Password, user.HashedPassword))
		{
			throw new AuthenticationServiceException("Аутентификация провалилась. Неверный логин или пароль.");
		}
		var n = new JwtProvider()
	}
}
