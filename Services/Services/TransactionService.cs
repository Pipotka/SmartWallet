using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Services.Contracts;
using Nasurino.SmartWallet.Services.Contracts.Models.Exceptions;
using Nasurino.SmartWallet.Services.Exceptions;
using Nasurino.SmartWallet.Services.Models;
using Nasurino.SmartWallet.Services.Models.CreateModels;
using Nasurino.SmartWallet.Services.Models.DeleteModels;
using Nasurino.SmartWallet.Services.Models.Models;

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

	async Task<PagedResultModel<TransactionModel>> ITransactionService.GetPagedListByUserIdAsync(Guid userId, TransactionQueryModel query, CancellationToken token)
	{
		await validateService.ValidateAsync(query, token);

		_ = await _userRepository.GetUserByIdAsync(userId, token)
		    ?? throw new EntityNotFoundByIdServiceException<User>(userId);

		var dalQuery = mapper.Map<TransactionQuery>(query);
		var pagedResult = await _transactionRepository.GetPagedListByUserIdAsync(userId, dalQuery, token);

		return mapper.Map<PagedResultModel<TransactionModel>>(pagedResult);
	}

	async Task<TransactionModel> ITransactionService.CreateAsync(CreateTransactionModel model, CancellationToken token)
	{
		await validateService.ValidateAsync(model, token);
		_ = await _userRepository.GetUserByIdAsync(model.UserId, token)
						?? throw new EntityNotFoundByIdServiceException<User>(model.UserId);
		
		var transaction = mapper.Map<Transaction>(model);
		TransactionEndpoint? sourceAccount = null;
		TransactionEndpoint? destinationAccount = null;

		if (model.SourceAccountId.HasValue)
		{
			sourceAccount = await _transactionEndpointRepository.GetByIdAndUserIdAsync(transaction.SourceAccountId!.Value,
									model.UserId,
									token)
							?? throw new EntityNotFoundByIdServiceException<TransactionEndpoint>(transaction.SourceAccountId!.Value);
		}
		if (model.DestinationAccountId.HasValue)
		{
			destinationAccount = await _transactionEndpointRepository.GetByIdAndUserIdAsync(transaction.DestinationAccountId!.Value,
				                    model.UserId,
				                    token)
			                    ?? throw new EntityNotFoundByIdServiceException<TransactionEndpoint>(transaction.DestinationAccountId!.Value);
		}

		if (sourceAccount != null)
		{
			if (!sourceAccount.IsStorage)
			{
				throw new SmartWalletValidationException(new PropertyValidationError(
					nameof(CreateTransactionModel.SourceAccountId),
					"Область трат не может быть указана как SourceAccount"));
			}

			var balanceResult = await _transactionRepository.GetBalanceByAccountIdAndDateRangeAsync(sourceAccount.Id, token) - transaction.Amount;
			
			if (sourceAccount.Limitation != null
				&& sourceAccount.Limitation > balanceResult
				&& destinationAccount is { IsStorage: false })
			{
				throw new AccountBalanceLimitViolationException(nameof(CreateTransactionModel.SourceAccountId), sourceAccount.Name);
			}
			
			sourceAccount.Value = balanceResult;
			_transactionEndpointRepository.Update(sourceAccount);
		}

		if (destinationAccount != null)
		{
			if (!model.SourceAccountId.HasValue && !destinationAccount.IsStorage)
			{
				throw new SmartWalletValidationException(new PropertyValidationError(
					nameof(CreateTransactionModel.DestinationAccountId),
					"Нельзя скорректировать баланс области трат"));
			}

			double currentBalace;
			if (destinationAccount.IsStorage)
			{
				currentBalace = await _transactionRepository.GetBalanceByAccountIdAndDateRangeAsync(destinationAccount.Id, token);
			}
			else
			{
				currentBalace = await _transactionRepository.GetBalanceByAccountIdAndDateRangeAsync(destinationAccount.Id,
				token,
				new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero),
				DateTimeOffset.UtcNow);
			}

			var balanceResult = currentBalace + transaction.Amount;
			if (destinationAccount.Limitation != null
				&& destinationAccount.Limitation < balanceResult
				&& destinationAccount is { IsStorage: false })
			{
				throw new AccountBalanceLimitViolationException(nameof(CreateTransactionModel.DestinationAccountId), destinationAccount.Name);
			}
			
			destinationAccount.Value = balanceResult;
			_transactionEndpointRepository.Update(destinationAccount);
			
		}
		
		if (sourceAccount is { IsStorage: true })
		{
			transaction.Type = TransactionType.Expense;
			
			if (destinationAccount == null)
			{
				transaction.Type = TransactionType.AdjustmentDecrease;
			}

			if (destinationAccount is { IsStorage: true })
			{
				transaction.Type = TransactionType.Transfer;
			}
		}
		else if (destinationAccount is { IsStorage: true })
		{
			transaction.Type = TransactionType.AdjustmentIncrease;
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
			throw new EntityNotFoundByIdServiceException<User>(model.UserId);
		}
		var transaction = await _transactionRepository.GetByIdAndUserIdAsync(model.Id, model.UserId, token)
			?? throw new EntityNotFoundByIdServiceException<Transaction>(model.Id);

		if (transaction.SourceAccountId.HasValue)
		{
			var sourceAccount = await _transactionEndpointRepository.GetByIdAndUserIdAsync(transaction.SourceAccountId!.Value,
				                    model.UserId,
				                    token);
			
			var balanceResult = await _transactionRepository.GetBalanceByAccountIdAndDateRangeAsync(sourceAccount!.Id, token) + transaction.Amount;
			sourceAccount.Value = balanceResult;
			_transactionEndpointRepository.Update(sourceAccount);
		}

		if (transaction.DestinationAccountId.HasValue)
		{
			var destinationAccount = await _transactionEndpointRepository.GetByIdAndUserIdAsync(
									transaction.DestinationAccountId!.Value,
									model.UserId,
									token);

			double currentBalace;
			if (destinationAccount is {IsStorage: true})
			{
				currentBalace = await _transactionRepository.GetBalanceByAccountIdAndDateRangeAsync(destinationAccount.Id, token);
			}
			else
			{
				currentBalace = await _transactionRepository.GetBalanceByAccountIdAndDateRangeAsync(destinationAccount.Id,
				token,
				new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero),
				DateTimeOffset.UtcNow);
			}

			var balanceResult = currentBalace - transaction.Amount;

			destinationAccount.Value = balanceResult;
			_transactionEndpointRepository.Update(destinationAccount);
			
		}

		_transactionRepository.Delete(transaction);
		await unitOfWork.SaveChangesAsync(token);
	}
}