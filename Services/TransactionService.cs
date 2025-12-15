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
public sealed class TransactionService(IUnitOfWork unitOfWork,
	ISmartWalletValidateService validateService,
	IMapper mapper) : ITransactionService
{
	private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
	private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;
	private readonly ITransactionEndpointRepository _transactionEndpointRepository = unitOfWork.TransactionEndpointRepository;

	async Task<List<TransactionModel>> ITransactionService.GetListByUserIdAsync(Guid userId, CancellationToken token)
	{
		var user = await _userRepository.GetUserByIdAsync(userId, token)
			?? throw new EntityNotFoundServiceException($"Пользователь с id = {userId} не найден.");

		var transactionList = await _transactionRepository.GetListByUserIdAsync(userId, token);

		return mapper.Map<List<TransactionModel>>(transactionList);
	}

	async Task<TransactionModel> ITransactionService.CreateAsync(CreateTransactionModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		if (await _userRepository.GetUserByIdAsync(model.UserId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {model.UserId} не найден.");
		}
		var transaction = mapper.Map<Transaction>(model);
		transaction.Id = Guid.NewGuid();

		if (model.SourceAccountId.HasValue)
		{
			var sourceAccount = await _transactionEndpointRepository.GetByIdAndUserIdAsync(transaction.SourceAccountId!.Value,
				                    model.UserId,
				                    token)
			                    ?? throw new EntityNotFoundServiceException($"Аккаунт источник с Id = {transaction.SourceAccountId} не найдена.");
			sourceAccount.Value -= transaction.Amount;
			_transactionEndpointRepository.Update(sourceAccount);
		}

		if (model.DestinationAccountId.HasValue)
		{
			var destinationAccount = await _transactionEndpointRepository.GetByIdAndUserIdAsync(transaction.DestinationAccountId!.Value,
				                         model.UserId,
				                         token)
			                         ?? throw new EntityNotFoundServiceException($"Аккаунт назначения с Id = {transaction.DestinationAccountId} не найдена.");
			destinationAccount.Value += transaction.Amount;
			_transactionEndpointRepository.Update(destinationAccount);
			
		}
		_transactionRepository.Add(transaction);
		
		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<TransactionModel>(transaction);
	}

	async Task ITransactionService.DeleteAsync(DeleteTransactionModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		if (await _userRepository.GetUserByIdAsync(model.UserId, token) is null)
		{
			throw new EntityNotFoundServiceException($"Пользователь с Id = {model.UserId} не найден.");
		}
		var transaction = await _transactionRepository.GetByIdAndUserIdAsync(model.Id, model.UserId, token)
			?? throw new EntityNotFoundServiceException($"Транзакция с Id = {model.Id} не найдена.");

		if (transaction.SourceAccountId.HasValue)
		{
			var sourceAccount = await _transactionEndpointRepository.GetByIdAndUserIdAsync(transaction.SourceAccountId!.Value,
				                    model.UserId,
				                    token);
			sourceAccount!.Value += transaction.Amount;
			_transactionEndpointRepository.Update(sourceAccount);
		}

		if (transaction.DestinationAccountId.HasValue)
		{
			var destinationAccount = await _transactionEndpointRepository.GetByIdAndUserIdAsync(
									transaction.DestinationAccountId!.Value,
									model.UserId,
									token);
			destinationAccount!.Value -= transaction.Amount;
			_transactionEndpointRepository.Update(destinationAccount);
			
		}

		_transactionRepository.Delete(transaction);
		await unitOfWork.SaveChangesAsync(token);
	}
}