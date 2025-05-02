using AutoMapper;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Nasurino.SmartWallet.Service.Models.UpdateModels;
using Nasurino.SmartWallet.Services.Validators;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис для работы с денежными хранилищами
/// </summary>
public class CashVaultService(UnitOfWork unitOfWork,
	SmartWalletValidateService validateService,
	IMapper mapper)
{
	private readonly UnitOfWork unitOfWork = unitOfWork;
	private readonly UserRepository userRepository = unitOfWork.UserRepository;
	private readonly CashVaultRepository cashVaultRepository = unitOfWork.CashVaultRepository;
	private readonly SmartWalletValidateService validateService = validateService;
	private readonly IMapper mapper = mapper;

	/// <summary>
	/// Возвращет список денежных хранилищ по идентификатору пользователя
	/// </summary>
	public async Task<List<CashVaultModel>> GetListByUserIdAsync(Guid userId, CancellationToken token)
	{
		var user = await userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с id = {userId} не найден.");

		var cashVaultList = await cashVaultRepository.GetListByUserIdAsync(userId, token);

		return mapper.Map<List<CashVaultModel>>(cashVaultList);
	}

	/// <summary>
	/// Создание нового денежного храшнилища
	/// </summary>
	public async Task<CashVaultModel> CreateAsync(CreateCashVaultModel model, CancellationToken token)
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

	/// <summary>
	/// Обновление денежного храшнилища
	/// </summary>
	public async Task<CashVaultModel> UpdateAsync(UpdateCashVaultModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(model.UserId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {model.UserId} не найден.");
		}
		var cashVault = await cashVaultRepository.GetByIdAsync(model.Id, token) 
			?? throw new EntityNotFoundServiceException($"Денежное хранилище с Id = {model.Id} не найдено.");
		if (cashVault.UserId != model.UserId)
		{
			throw new EntityAccessServiceException($"Пользователь с Id = {model.UserId} не является владельцем денежного храшнилища.");
		}
		mapper.Map(model, cashVault);
		cashVaultRepository.Update(cashVault);

		await unitOfWork.SaveChangesAsync(token);
		return mapper.Map<CashVaultModel>(cashVault);
	}

	/// <summary>
	/// Удаление денежного храшнилища
	/// </summary>
	public async Task DeleteAsync(Guid userId, DeleteCashVaultModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(userId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {userId} не найден.");
		}
		var cashVault = await cashVaultRepository.GetByIdAsync(model.Id, token)
			?? throw new EntityNotFoundServiceException($"Денежное хранилище с Id = {model.Id} не найдено.");

		if (cashVault.UserId != userId)
		{
			throw new EntityAccessServiceException($"Пользователь с Id = {userId} не является владельцем денежного храшнилища.");
		}

		cashVaultRepository.Delete(cashVault);

		await unitOfWork.SaveChangesAsync(token);
	}
}