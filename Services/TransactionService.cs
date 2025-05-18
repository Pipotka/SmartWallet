using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Services.Contracts;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис для работы с транзакциями
/// </summary>
public class TransactionService(IUnitOfWork unitOfWork,
	ISmartWalletValidateService validateService,
	IMapper mapper) : ITransactionService
{
	private readonly IUnitOfWork unitOfWork = unitOfWork;
	private readonly IUserRepository userRepository = unitOfWork.UserRepository;
	private readonly ITransactionRepository transactionRepository = unitOfWork.TransactionRepository;
	private readonly ICashVaultRepository cashVaultRepository = unitOfWork.CashVaultRepository;
	private readonly ISpendingAreaRepository spendingAreaRepository = unitOfWork.SpendingAreaRepository;
	private readonly ISmartWalletValidateService validateService = validateService;
	private readonly IMapper mapper = mapper;

	async Task<List<TransactionModel>> ITransactionService.GetListByUserIdAsync(Guid userId, CancellationToken token)
	{
		var user = await userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с id = {userId} не найден.");

		var transactionList = await transactionRepository.GetListByUserIdAsync(userId, token);

		return mapper.Map<List<TransactionModel>>(transactionList);
	}

	async Task<TransactionModel> ITransactionService.CreateAsync(Guid userId, CreateTransactionModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(userId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {userId} не найден.");
		}
		var transaction = mapper.Map<Transaction>(model);
		transaction.Id = Guid.NewGuid();
		transaction.UserId = userId;

		var cashVault = await cashVaultRepository.GetByIdAndUserIdAsync(transaction.FromCashVaultId, userId, token)
			?? throw new EntityNotFoundServiceException($"Денежное хранилище с Id = {transaction.FromCashVaultId} не найдена.");
		var spendingArea = await spendingAreaRepository.GetByIdAndUserIdAsync(transaction.ToSpendingAreaId, userId, token)
			?? throw new EntityNotFoundServiceException($"Область трат с Id = {transaction.ToSpendingAreaId} не найдена.");

		cashVault.Value -= transaction.Value;
		spendingArea.Value += transaction.Value;

		transactionRepository.Add(transaction);
		cashVaultRepository.Update(cashVault);
		spendingAreaRepository.Update(spendingArea);
		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<TransactionModel>(transaction);
	}

	async Task ITransactionService.DeleteAsync(Guid userId, DeleteTransactionModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(userId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {userId} не найден.");
		}
		var transaction = await transactionRepository.GetByIdAndUserIdAsync(model.Id, userId, token)
			?? throw new EntityNotFoundServiceException($"Транзакция с Id = {model.Id} не найдена.");

		var cashVault = await cashVaultRepository.GetByIdAndUserIdAsync(transaction.FromCashVaultId, userId, token);
		var spendingArea = await spendingAreaRepository.GetByIdAndUserIdAsync(transaction.ToSpendingAreaId, userId, token);

		if (cashVault is not null)
		{
			cashVault.Value += transaction.Value;
			cashVaultRepository.Update(cashVault);
		}
		if (spendingArea is not null)
		{
			spendingArea.Value -= transaction.Value;
			spendingAreaRepository.Update(spendingArea);
		}

		transactionRepository.Delete(transaction);
		await unitOfWork.SaveChangesAsync(token);
	}
}