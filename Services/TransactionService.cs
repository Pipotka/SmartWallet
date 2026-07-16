using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Services.Contracts;
using Nasurino.SmartWallet.Services.Contracts.BackgroundService;
using Services.Contracts.Models.Exceptions;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис для работы с транзакциями
/// </summary>
public sealed class TransactionService(IUnitOfWork unitOfWork,
	ISmartWalletValidateService validateService,
	IMapper mapper,
	IDailyExpenseCategorieService dailyExpenseCategorieService) : ITransactionService
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
			sourceAccount = await _transactionEndpointRepository.GetByIdAndUserIdAsync(model.SourceAccountId!.Value,
				model.UserId, token)
				?? throw new EntityNotFoundByIdServiceException<TransactionEndpoint>(model.SourceAccountId!.Value);
		}
		if (model.DestinationAccountId.HasValue)
		{
			destinationAccount = await _transactionEndpointRepository.GetByIdAndUserIdAsync(model.DestinationAccountId!.Value,
				model.UserId, token)
				?? throw new EntityNotFoundByIdServiceException<TransactionEndpoint>(model.DestinationAccountId!.Value);
		}

		if (sourceAccount != null)
		{
			if (!sourceAccount.IsStorage)
			{
				throw new SmartWalletValidationException(new PropertyValidationError(
					nameof(CreateTransactionModel.SourceAccountId),
					"Область трат не может быть указана как SourceAccount"));
			}

			var balanceResult = await _transactionRepository.GetBalanceByAccountIdAndDateRangeAsync(sourceAccount.Id, token) - model.Amount;

			if (sourceAccount.Limitation != null
				&& sourceAccount.Limitation > balanceResult
				&& destinationAccount is { IsStorage: false })
			{
				throw new AccountBalanceLimitViolationException(nameof(CreateTransactionModel.SourceAccountId), sourceAccount.Name);
			}
		}

		if (destinationAccount != null)
		{
			if (!model.SourceAccountId.HasValue && !destinationAccount.IsStorage)
			{
				throw new SmartWalletValidationException(new PropertyValidationError(
					nameof(CreateTransactionModel.DestinationAccountId),
					"Нельзя скорректировать баланс области трат"));
			}

			double currentBalance;
			if (destinationAccount.IsStorage)
			{
				currentBalance = await _transactionRepository.GetBalanceByAccountIdAndDateRangeAsync(destinationAccount.Id, token);
			}
			else
			{
				currentBalance = await _transactionRepository.GetBalanceByAccountIdAndDateRangeAsync(destinationAccount.Id,
					token,
					new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero),
					DateTimeOffset.UtcNow);
			}

			var balanceResult = currentBalance + model.Amount;
			if (destinationAccount.Limitation != null
				&& destinationAccount.Limitation < balanceResult
				&& destinationAccount is { IsStorage: false })
			{
				throw new AccountBalanceLimitViolationException(nameof(CreateTransactionModel.DestinationAccountId), destinationAccount.Name);
			}
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

		transaction.Postings = BuildPostings(sourceAccount, destinationAccount, model.Amount);

		_transactionRepository.Add(transaction);

		await unitOfWork.SaveChangesAsync(token);

		if (sourceAccount != null)
		{
			await _transactionEndpointRepository.RecalculateValueAsync(sourceAccount.Id, token);
		}
		if (destinationAccount != null)
		{
			await _transactionEndpointRepository.RecalculateValueAsync(destinationAccount.Id, token);
		}

		await unitOfWork.SaveChangesAsync(token);

		await dailyExpenseCategorieService.RecalculateForTransactionAsync(transaction, token);

		await unitOfWork.SaveChangesAsync(token);

		return mapper.Map<TransactionModel>(transaction);
	}

	private static ICollection<Posting> BuildPostings(TransactionEndpoint? source, TransactionEndpoint? destination, double amount)
	{
		var postings = new List<Posting>();

		if (source != null)
		{
			postings.Add(new Posting
			{
				AccountId = source.Id,
				Amount = -amount
			});
		}

		if (destination != null)
		{
			postings.Add(new Posting
			{
				AccountId = destination.Id,
				Amount = amount
			});
		}

		return postings;
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

		var affectedAccountIds = transaction.Postings
			.Select(p => p.AccountId)
			.ToHashSet();

		await dailyExpenseCategorieService.RecalculateForTransactionAsync(transaction, token);

		_transactionRepository.Delete(transaction);
		await unitOfWork.SaveChangesAsync(token);

		foreach (var accountId in affectedAccountIds)
		{
			await _transactionEndpointRepository.RecalculateValueAsync(accountId, token);
		}

		await unitOfWork.SaveChangesAsync(token);
	}
}
