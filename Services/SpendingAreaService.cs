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
/// Сервис для работы с областями трат
/// </summary>
public class SpendingAreaService(UnitOfWork unitOfWork,
	SmartWalletValidateService validateService,
	IMapper mapper)
{
	private readonly UnitOfWork unitOfWork = unitOfWork;
	private readonly UserRepository userRepository = unitOfWork.UserRepository;
	private readonly SpendingAreaRepository SpendingAreaRepository = unitOfWork.SpendingAreaRepository;
	private readonly SmartWalletValidateService validateService = validateService;
	private readonly IMapper mapper = mapper;

	/// <summary>
	/// Возвращет список областей трат по идентификатору пользователя
	/// </summary>
	public async Task<List<SpendingAreaModel>> GetListByUserIdAsync(Guid userId, CancellationToken token)
	{
		var user = await userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с id = {userId} не найден.");

		var spendingAreaList = await SpendingAreaRepository.GetListByUserIdAsync(userId, token);

		// [TODO] Написать маппер
		return mapper.Map<List<SpendingAreaModel>>(spendingAreaList);
	}

	/// <summary>
	/// Создание новой области трат
	/// </summary>
	public async Task<SpendingAreaModel> CreateAsync(Guid userId, CreateSpendingAreaModel model, CancellationToken token)
	{
		// [TODO] создать валидатор
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(userId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {userId} не найден.");
		}
		// [TODO] Написать маппер
		var SpendingArea = mapper.Map<SpendingArea>(model);
		SpendingArea.Id = Guid.NewGuid();
		SpendingArea.UserId = userId;
		SpendingAreaRepository.Add(SpendingArea);

		await unitOfWork.SaveChangesAsync(token);

		// [TODO] Написать маппер
		return mapper.Map<SpendingAreaModel>(SpendingArea);
	}

	/// <summary>
	/// Обновление области трат
	/// </summary>
	public async Task<SpendingAreaModel> UpdateAsync(Guid userId, UpdateSpendingAreaModel model, CancellationToken token)
	{
		// [TODO] создать валидатор
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(userId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {userId} не найден.");
		}
		var SpendingArea = await SpendingAreaRepository.GetByIdAsync(model.Id, token)
			?? throw new EntityNotFoundServiceException($"Область трат с Id = {model.Id} не найдено.");
		if (SpendingArea.UserId != userId)
		{
			throw new EntityAccessServiceException($"Пользователь с Id = {userId} не является владельцем области трат.");
		}
		// [TODO] Написать маппер
		mapper.Map(model, SpendingArea);
		SpendingAreaRepository.Update(SpendingArea);

		await unitOfWork.SaveChangesAsync(token);
		// [TODO] Написать маппер
		return mapper.Map<SpendingAreaModel>(SpendingArea);
	}

	/// <summary>
	/// Удаление области трат
	/// </summary>
	public async Task DeleteAsync(Guid userId, DeleteSpendingAreaModel model, CancellationToken token)
	{
		// [TODO] создать валидатор
		await validateService.ValidateAsync(model, token);
		if (await userRepository.GetUserByIdAsync(userId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {userId} не найден.");
		}
		var SpendingArea = await SpendingAreaRepository.GetByIdAsync(model.Id, token)
			?? throw new EntityNotFoundServiceException($"Область трат с Id = {model.Id} не найдено.");

		if (SpendingArea.UserId != userId)
		{
			throw new EntityAccessServiceException($"Пользователь с Id = {userId} не является владельцем области трат.");
		}

		SpendingAreaRepository.Delete(SpendingArea);

		await unitOfWork.SaveChangesAsync(token);
	}
}