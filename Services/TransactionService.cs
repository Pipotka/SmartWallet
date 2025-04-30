using AutoMapper;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Services.Validators;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис для работы с транзакциями
/// </summary>
public class TransactionService(UnitOfWork unitOfWork,
	SmartWalletValidateService validateService,
	IMapper mapper)
{
	private readonly UnitOfWork unitOfWork = unitOfWork;
	private readonly UserRepository userRepository = unitOfWork.UserRepository;
	private readonly TransactionRepository TransactionRepository = unitOfWork.TransactionRepository;
	private readonly SmartWalletValidateService validateService = validateService;
	private readonly IMapper mapper = mapper;

	/// <summary>
	/// Возвращет список транзакций по идентификатору пользователя
	/// </summary>
	public async Task<List<TransactionModel>> GetListByUserIdAsync(Guid userId, CancellationToken token)
	{
		var user = await userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с id = {userId} не найден.");

		var transactionList = await TransactionRepository.GetListByUserIdAsync(userId, token);

		// [TODO] Написать маппер
		return mapper.Map<List<TransactionModel>>(transactionList);
	}

	/// <summary>
	/// Создание новой транзакции
	/// </summary>
	public async Task<TransactionModel> CreateAsync(Guid userId, CreateTransactionModel model, CancellationToken token)
	{
		// [TODO] создать валидатор
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(userId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {userId} не найден.");
		}
		// [TODO] Написать маппер
		var Transaction = mapper.Map<Transaction>(model);
		Transaction.Id = Guid.NewGuid();
		Transaction.UserId = userId;
		TransactionRepository.Add(Transaction);

		await unitOfWork.SaveChangesAsync(token);

		// [TODO] Написать маппер
		return mapper.Map<TransactionModel>(Transaction);
	}

	/// <summary>
	/// Удаление транзакции
	/// </summary>
	public async Task DeleteAsync(Guid userId, DeleteTransactionModel model, CancellationToken token)
	{
		// [TODO] создать валидатор
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(userId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {userId} не найден.");
		}
		var Transaction = await TransactionRepository.GetByIdAsync(model.Id, token)
			?? throw new EntityNotFoundServiceException($"Транзакция с Id = {model.Id} не найдена.");

		if (Transaction.UserId != userId)
		{
			throw new EntityAccessServiceException($"Пользователь с Id = {userId} не является владельцем транзакции.");
		}

		TransactionRepository.Delete(Transaction);

		await unitOfWork.SaveChangesAsync(token);
	}
}