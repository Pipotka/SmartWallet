using AutoMapper;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.BackgroundTaskSystem.Contracts;
using Nasurino.SmartWallet.Services.Contracts.BackgroundService;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Services.Contracts;
using Services.Contracts.Models.Exceptions;

namespace Nasurino.SmartWallet.Services;

/// <summary>
/// Сервис для работы с транзакциями
/// </summary>
public sealed class TransactionService(IUnitOfWork unitOfWork,
	ISmartWalletValidateService validateService,
	IMapper mapper,
	IBackgroundTaskSystemProvider backgroundTaskSystemProvider) : ITransactionService
{
	private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
	private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;
	private readonly ITransactionEndpointRepository _transactionEndpointRepository = unitOfWork.TransactionEndpointRepository;
	private readonly IPostingRepository _postingRepository = unitOfWork.PostingRepository;
	private readonly IBackgroundTaskSystemProvider _backgroundTaskSystemProvider = backgroundTaskSystemProvider;

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

		TransactionEndpoint? sourceAccount = null;
		TransactionEndpoint? destinationAccount = null;

		if (model.SourceAccountId.HasValue)
		{
			sourceAccount = await _transactionEndpointRepository.GetByIdAndUserIdAsync(
				model.SourceAccountId.Value,
				model.UserId,
				token)
				?? throw new EntityNotFoundByIdServiceException<TransactionEndpoint>(model.SourceAccountId.Value);
		}

		if (model.DestinationAccountId.HasValue)
		{
			destinationAccount = await _transactionEndpointRepository.GetByIdAndUserIdAsync(
				model.DestinationAccountId.Value,
				model.UserId,
				token)
				?? throw new EntityNotFoundByIdServiceException<TransactionEndpoint>(model.DestinationAccountId.Value);
		}

		var amount = model.Amount;

		if (sourceAccount != null)
		{
			if (!sourceAccount.IsStorage)
			{
				throw new SmartWalletValidationException(new PropertyValidationError(
					nameof(CreateTransactionModel.SourceAccountId),
					"Область трат не может быть указана как SourceAccount"));
			}

			var balanceResult = await _transactionRepository.GetBalanceByAccountIdAndDateRangeAsync(
				sourceAccount.Id,
				token) - amount;

			if (sourceAccount.Limitation != null
				&& sourceAccount.Limitation > balanceResult
				&& destinationAccount is { IsStorage: false })
			{
				throw new AccountBalanceLimitViolationException(
					nameof(CreateTransactionModel.SourceAccountId),
					sourceAccount.Name);
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

			var currentBalance = await GetBalanceForAccountAsync(destinationAccount.Id, destinationAccount.IsStorage, token);

			var balanceResult = currentBalance + amount;
			if (destinationAccount.Limitation != null
				&& destinationAccount.Limitation < balanceResult
				&& destinationAccount is { IsStorage: false })
			{
				throw new AccountBalanceLimitViolationException(
					nameof(CreateTransactionModel.DestinationAccountId),
					destinationAccount.Name);
			}

			destinationAccount.Value = balanceResult;
			_transactionEndpointRepository.Update(destinationAccount);
		}

		var transactionId = Guid.NewGuid();
		var transaction = new Transaction
		{
			Id = transactionId,
			UserId = model.UserId,
			Type = ResolveType(sourceAccount, destinationAccount),
			Postings = BuildPostings(sourceAccount, destinationAccount, amount, transactionId)
		};

		_transactionRepository.Add(transaction);
		_postingRepository.AddRange(transaction.Postings);
		await unitOfWork.SaveChangesAsync(token);

		if (destinationAccount is { IsStorage: false })
		{
			var categoryId = destinationAccount.Id;
			var day = DateTime.UtcNow.Date;
			_backgroundTaskSystemProvider.FireAndForget<IDailyExpenseCategorieRecalculationService>(s =>
				s.RecalculateAsync(model.UserId, categoryId, day, token));
		}

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

		var affectedCategories = new HashSet<Guid>();

		var accountIds = transaction.Postings
			.Select(p => p.AccountId)
			.Distinct()
			.ToList();

		var endpoints = await _transactionEndpointRepository.GetListByIdsAndUserIdAsync(
			model.UserId,
			accountIds,
			token);

		if (endpoints.Count > 0)
		{
			var storageIds = endpoints
				.Where(e => e.IsStorage)
				.Select(e => e.Id)
				.ToList();
			var categoryIds = endpoints
				.Where(e => !e.IsStorage)
				.Select(e => e.Id)
				.ToList();

			var storageBalances = await _transactionRepository.GetStorageBalancesAsync(storageIds, token);
			var categoryBalances = await _transactionRepository.GetCategoryBalancesAsync(categoryIds, token);

			var endpointById = endpoints.ToDictionary(e => e.Id);

			foreach (var posting in transaction.Postings)
			{
				if (!endpointById.TryGetValue(posting.AccountId, out var account))
				{
					continue;
				}

				var currentBalance = account.IsStorage
					? storageBalances.TryGetValue(account.Id, out var sb) ? sb : 0m
					: categoryBalances.TryGetValue(account.Id, out var cb) ? cb : 0m;
				account.Value = currentBalance - posting.Amount;
				_transactionEndpointRepository.Update(account);

				posting.DeletedAt = DateTimeOffset.UtcNow;
				_postingRepository.Update(posting);

				if (account is { IsStorage: false })
				{
					affectedCategories.Add(account.Id);
				}
			}
		}

		_transactionRepository.Delete(transaction);
		await unitOfWork.SaveChangesAsync(token);

		if (affectedCategories.Count > 0)
		{
			var day = transaction.MadeAt.Date;

			_backgroundTaskSystemProvider.FireAndForget<IDailyExpenseCategorieRecalculationService>(s =>
				s.RecalculateManyAsync(model.UserId, affectedCategories, day, token));
		}
	}

	private async Task<decimal> GetBalanceForAccountAsync(
		Guid accountId,
		bool isStorage,
		CancellationToken token)
	{
		if (isStorage)
		{
			return await _transactionRepository.GetBalanceByAccountIdAndDateRangeAsync(accountId, token);
		}

		var startOfMonth = new DateTimeOffset(
			DateTimeOffset.UtcNow.Year,
			DateTimeOffset.UtcNow.Month,
			1,
			0, 0, 0,
			TimeSpan.Zero);

		return await _transactionRepository.GetBalanceByAccountIdAndDateRangeAsync(
			accountId,
			token,
			startOfMonth,
			DateTimeOffset.UtcNow);
	}

	private static TransactionType ResolveType(TransactionEndpoint? source, TransactionEndpoint? destination)
	{
		if (source is { IsStorage: true })
		{
			if (destination is null)
			{
				return TransactionType.AdjustmentDecrease;
			}

			return destination is { IsStorage: true }
				? TransactionType.Transfer
				: TransactionType.Expense;
		}

		return destination is { IsStorage: true }
			? TransactionType.AdjustmentIncrease
			: TransactionType.Expense;
	}

	private static List<Posting> BuildPostings(
		TransactionEndpoint? source,
		TransactionEndpoint? destination,
		decimal amount,
		Guid transactionId)
	{
		var postings = new List<Posting>();

		if (source is { IsStorage: true })
		{
			postings.Add(new Posting
			{
				TransactionId = transactionId,
				Transaction = null,
				AccountId = source.Id,
				Amount = -amount
			});
		}

		if (destination != null)
		{
			postings.Add(new Posting
			{
				TransactionId = transactionId,
				Transaction = null,
				AccountId = destination.Id,
				Amount = amount
			});
		}

		return postings;
	}
}
