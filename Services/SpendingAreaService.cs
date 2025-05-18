using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Services.Contracts;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис для работы с областями трат
/// </summary>
public class SpendingAreaService(IUnitOfWork unitOfWork,
	ISmartWalletValidateService validateService,
	IMapper mapper) : ISpendingAreaService
{
	private readonly IUnitOfWork unitOfWork = unitOfWork;
	private readonly IUserRepository userRepository = unitOfWork.UserRepository;
	private readonly ISpendingAreaRepository spendingAreaRepository = unitOfWork.SpendingAreaRepository;
	private readonly ICashVaultRepository cashVaultRepository = unitOfWork.CashVaultRepository;
	private readonly ITransactionRepository transactionRepository = unitOfWork.TransactionRepository;
	private readonly ISmartWalletValidateService validateService = validateService;
	private readonly IMapper mapper = mapper;

	async Task<List<SpendingAreaModel>> ISpendingAreaService.GetListByUserIdAsync(Guid userId, CancellationToken token)
	{
		var user = await userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с id = {userId} не найден.");

		var spendingAreaList = await spendingAreaRepository.GetListByUserIdAsync(userId, token);

		return mapper.Map<List<SpendingAreaModel>>(spendingAreaList);
	}

	async Task<SpendingAreaModel> ISpendingAreaService.CreateAsync(CreateSpendingAreaModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(model.UserId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {model.UserId} не найден.");
		}
		var SpendingArea = mapper.Map<SpendingArea>(model);
		SpendingArea.Id = Guid.NewGuid();
		SpendingArea.UserId = model.UserId;
		spendingAreaRepository.Add(SpendingArea);

		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<SpendingAreaModel>(SpendingArea);
	}

	async Task<SpendingAreaModel> ISpendingAreaService.UpdateAsync(UpdateSpendingAreaModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(model.UserId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {model.UserId} не найден.");
		}
		var SpendingArea = await spendingAreaRepository.GetByIdAndUserIdAsync(model.Id, model.UserId, token)
			?? throw new EntityNotFoundServiceException($"Область трат с Id = {model.Id} не найдено.");

		mapper.Map(model, SpendingArea);
		spendingAreaRepository.Update(SpendingArea);

		await unitOfWork.SaveChangesAsync(token);
		return mapper.Map<SpendingAreaModel>(SpendingArea);
	}

	async Task ISpendingAreaService.DeleteAsync(Guid userId, DeleteSpendingAreaModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(userId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {userId} не найден.");
		}
		var spendingArea = await spendingAreaRepository.GetByIdAndUserIdAsync(model.Id, userId, token)
			?? throw new EntityNotFoundServiceException($"Область трат с Id = {model.Id} не найдено.");

		spendingAreaRepository.Delete(spendingArea);

		var transactions = await transactionRepository.GetListByUserIdAsync(userId, token);
		var cashVaults = (await cashVaultRepository.GetListByUserIdAsync(userId, token)).ToDictionary(x => x.Id, x => x);
		foreach (var transaction in transactions)
		{
			if (cashVaults.TryGetValue(transaction.FromCashVaultId, out var cashVault))
			{
				cashVault.Value += transaction.Value;
				cashVaultRepository.Update(cashVault);
			}
		}
		transactionRepository.DeleteTransactionsBySpendingAreaId(spendingArea.Id);

		await unitOfWork.SaveChangesAsync(token);
	}
}