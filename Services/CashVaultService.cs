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
/// Сервис для работы с денежными хранилищами
/// </summary>
public class CashVaultService(IUnitOfWork unitOfWork,
	ISmartWalletValidateService validateService,
	IMapper mapper) : ICashVaultService
{
	private readonly IUnitOfWork unitOfWork = unitOfWork;
	private readonly IUserRepository userRepository = unitOfWork.UserRepository;
	private readonly ICashVaultRepository cashVaultRepository = unitOfWork.CashVaultRepository;
	private readonly ISmartWalletValidateService validateService = validateService;
	private readonly IMapper mapper = mapper;

	async Task<List<CashVaultModel>> ICashVaultService.GetListByUserIdAsync(Guid userId, CancellationToken token)
	{
		var user = await userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с id = {userId} не найден.");

		var cashVaultList = await cashVaultRepository.GetListByUserIdAsync(userId, token);

		return mapper.Map<List<CashVaultModel>>(cashVaultList);
	}

	async Task<CashVaultModel> ICashVaultService.CreateAsync(CreateCashVaultModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(model.UserId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {model.UserId} не найден.");
		}
		var cashVault = mapper.Map<CashVault>(model);
		cashVault.Id = Guid.NewGuid();
		cashVault.UserId = model.UserId;
		cashVaultRepository.Add(cashVault);

		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<CashVaultModel>(cashVault);
	}

	async Task<CashVaultModel> ICashVaultService.UpdateAsync(UpdateCashVaultModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(model.UserId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {model.UserId} не найден.");
		}
		var cashVault = await cashVaultRepository.GetByIdAndUserIdAsync(model.Id, model.UserId, token)
			?? throw new EntityNotFoundServiceException($"Денежное хранилище с Id = {model.Id} не найдено.");

		mapper.Map(model, cashVault);
		cashVaultRepository.Update(cashVault);

		await unitOfWork.SaveChangesAsync(token);
		return mapper.Map<CashVaultModel>(cashVault);
	}

	async Task ICashVaultService.DeleteAsync(Guid userId, DeleteCashVaultModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(userId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {userId} не найден.");
		}
		var cashVault = await cashVaultRepository.GetByIdAndUserIdAsync(model.Id, userId, token)
			?? throw new EntityNotFoundServiceException($"Денежное хранилище с Id = {model.Id} не найдено.");

		cashVaultRepository.Delete(cashVault);

		await unitOfWork.SaveChangesAsync(token);
	}
}