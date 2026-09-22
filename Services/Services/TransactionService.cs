using AutoMapper;
using Microsoft.Extensions.Options;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.BackgroundTaskSystem.Contracts;
using Nasurino.SmartWallet.Services.Contracts.BackgroundService;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Options;
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
public sealed class TransactionService(
    IUnitOfWork unitOfWork,
    ISmartWalletValidateService validateService,
    IMapper mapper,
    IBackgroundTaskSystemProvider backgroundTaskSystemProvider,
    IOptions<PostingSettings> postingSettings) : ITransactionService
{
    private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
    private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;
    private readonly ITransactionEndpointRepository _transactionEndpointRepository = unitOfWork.TransactionEndpointRepository;
    private readonly IPostingRepository _postingRepository = unitOfWork.PostingRepository;
    private readonly IBackgroundTaskSystemProvider _backgroundTaskSystemProvider = backgroundTaskSystemProvider;
    private readonly PostingSettings _postingSettings = postingSettings.Value;

    /// <inheritdoc/>
    async Task<PagedResultModel<TransactionModel>> ITransactionService.GetPagedListByUserIdAsync(
        Guid userId,
        TransactionQueryModel query,
        CancellationToken token)
    {
        await validateService.ValidateAsync(query, token);

        _ = await _userRepository.GetUserByIdAsync(userId, token)
            ?? throw new EntityNotFoundByIdServiceException<User>(userId);

        var dalQuery = mapper.Map<TransactionQuery>(query);
        var pagedResult = await _transactionRepository.GetPagedListByUserIdAsync(userId, dalQuery, token);

        return mapper.Map<PagedResultModel<TransactionModel>>(pagedResult);
    }

    /// <inheritdoc/>
    async Task<TransactionModel> ITransactionService.CreateAsync(CreateTransactionModel model, CancellationToken token)
    {
        await validateService.ValidateAsync(model, token);

        _ = await _userRepository.GetUserByIdAsync(model.UserId, token)
            ?? throw new EntityNotFoundByIdServiceException<User>(model.UserId);

        var maxPostings = _postingSettings.MaxPostingsPerTransaction;

        ValidatePostings(model.Postings, maxPostings);

        var accountIds = model.Postings.Select(p => p.AccountId).ToList();
        var endpoints = await _transactionEndpointRepository.GetListByIdsAndUserIdAsync(model.UserId, accountIds, token);
        var endpointsById = endpoints.ToDictionary(e => e.Id);

        ValidateAccounts(model.Postings, endpointsById);

        var type = ClassifyTransaction(model.Postings, endpointsById);

        var transactionId = Guid.NewGuid();
        var transaction = new Transaction
        {
            Id = transactionId,
            UserId = model.UserId,
            Type = type,
            Postings = BuildPostings(model.Postings, transactionId)
        };

        var affectedCategoryIds = await ApplyBalanceUpdatesAsync(transaction.Postings.ToList(), endpointsById, token);

        _transactionRepository.Add(transaction);
        _postingRepository.AddRange(transaction.Postings);
        await unitOfWork.SaveChangesAsync(token);

        if (affectedCategoryIds.Count > 0)
        {
            var day = DateTime.UtcNow.Date;
            _backgroundTaskSystemProvider.FireAndForget<IDailyExpenseCategorieRecalculationService>(s =>
                s.RecalculateManyAsync(model.UserId, affectedCategoryIds, day, token));
        }

        return mapper.Map<TransactionModel>(transaction);
    }

    /// <inheritdoc/>
    async Task ITransactionService.DeleteAsync(DeleteTransactionModel model, CancellationToken token)
    {
        await validateService.ValidateAsync(model, token);

        if (await _userRepository.GetUserByIdAsync(model.UserId, token) is null)
        {
            throw new EntityNotFoundByIdServiceException<User>(model.UserId);
        }

        var transaction = await _transactionRepository.GetByIdAndUserIdAsync(model.Id, model.UserId, token)
            ?? throw new EntityNotFoundByIdServiceException<Transaction>(model.Id);

        var accountIds = transaction.Postings
            .Select(p => p.AccountId)
            .Distinct()
            .ToList();

        var endpoints = await _transactionEndpointRepository.GetListByIdsAndUserIdAsync(model.UserId, accountIds, token);
        var endpointById = endpoints.ToDictionary(e => e.Id);

        var affectedCategories = new HashSet<Guid>();

        var storageIds = endpoints
            .Where(e => e.EndpointType == EndpointType.Storage)
            .Select(e => e.Id)
            .ToList();

        var categoryIds = endpoints
            .Where(e => e.EndpointType == EndpointType.Category)
            .Select(e => e.Id)
            .ToList();

        var storageBalances = await _transactionRepository.GetStorageBalancesAsync(storageIds, token);
        var categoryBalances = await _transactionRepository.GetCategoryBalancesAsync(categoryIds, token);

        foreach (var posting in transaction.Postings)
        {
            if (!endpointById.TryGetValue(posting.AccountId, out var account))
            {
                continue;
            }

            var currentBalance = account.EndpointType == EndpointType.Storage
                ? storageBalances.TryGetValue(account.Id, out var sb) ? sb : 0m
                : categoryBalances.TryGetValue(account.Id, out var cb) ? cb : 0m;

            account.Value = currentBalance - posting.Amount;
            _transactionEndpointRepository.Update(account);

            posting.DeletedAt = DateTimeOffset.UtcNow;
            _postingRepository.Update(posting);

            if (account.EndpointType == EndpointType.Category)
            {
                affectedCategories.Add(account.Id);
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

    /// <summary>
    /// Валидация проводок создаваемой транзакции
    /// </summary>
    private static void ValidatePostings(List<CreateTransactionPostingModel> postings, int maxPostings)
    {
        if (postings == null || postings.Count == 0)
        {
            throw new SmartWalletValidationException(ErrorCodes.PostingsEmpty, "Список проводок пуст");
        }

        if (postings.Count > maxPostings)
        {
            throw new SmartWalletValidationException(ErrorCodes.PostingsLimitExceeded,
                $"Превышен лимит проводок ({maxPostings})");
        }

        var seenAccounts = new HashSet<Guid>();

        foreach (var posting in postings)
        {
            if (posting.AccountId == Guid.Empty)
            {
                throw new SmartWalletValidationException(ErrorCodes.InvalidAccountId,
                    "Идентификатор счета не может быть пустым");
            }

            if (posting.Amount == 0)
            {
                throw new SmartWalletValidationException(ErrorCodes.ZeroAmount,
                    "Сумма проводки не может быть равна нулю");
            }

            if (!seenAccounts.Add(posting.AccountId))
            {
                throw new SmartWalletValidationException(ErrorCodes.DuplicateAccountId,
                    $"Счет {posting.AccountId} указан более одного раза");
            }
        }
    }

    /// <summary>
    /// Валидация принадлежности счетов пользователю
    /// </summary>
    private static void ValidateAccounts(
        List<CreateTransactionPostingModel> postings,
        Dictionary<Guid, TransactionEndpoint> endpointsById)
    {
        foreach (var posting in postings)
        {
            if (!endpointsById.TryGetValue(posting.AccountId, out _))
            {
                throw new EntityNotFoundByIdServiceException<TransactionEndpoint>(posting.AccountId);
            }
        }
    }

    /// <summary>
    /// Определяет тип транзакции по проводкам. Не создаёт системных проводок.
    /// </summary>
    private static TransactionType ClassifyTransaction(
        List<CreateTransactionPostingModel> postings,
        Dictionary<Guid, TransactionEndpoint> endpointsById)
    {
        var userSum = postings.Sum(p => p.Amount);
        var storagePostings = postings
            .Where(p => endpointsById[p.AccountId].EndpointType == EndpointType.Storage)
            .ToList();

        var categoryPostings = postings
            .Where(p => endpointsById[p.AccountId].EndpointType == EndpointType.Category)
            .ToList();

        if (categoryPostings.Count > 0)
        {
            if (storagePostings.Count > 0
                && storagePostings.All(p => p.Amount < 0)
                && categoryPostings.All(p => p.Amount > 0)
                && userSum == 0)
            {
                return TransactionType.Expense;
            }

            throw new SmartWalletValidationException(ErrorCodes.InvalidPostingCombination,
                "Комбинация проводок не соответствует ни одному типу транзакции");
        }

        var signs = storagePostings
            .Select(p => Math.Sign(p.Amount))
            .Distinct()
            .ToHashSet();

        if (signs.Count == 2 && userSum == 0)
        {
            return TransactionType.Transfer;
        }

        if (signs.Count == 1 && signs.Contains(1) && userSum > 0)
        {
            return TransactionType.AdjustmentIncrease;
        }

        if (signs.Count == 1 && signs.Contains(-1) && userSum < 0)
        {
            return TransactionType.AdjustmentDecrease;
        }

        throw new SmartWalletValidationException(ErrorCodes.InvalidPostingCombination,
            "Комбинация проводок не соответствует ни одному типу транзакции");
    }

    /// <summary>
    /// Формирует итоговый список проводок
    /// </summary>
    private static List<Posting> BuildPostings(
        List<CreateTransactionPostingModel> userPostings,
        Guid transactionId)
    {
        return userPostings
            .Select(p => new Posting
            {
                Id = Guid.NewGuid(),
                TransactionId = transactionId,
                AccountId = p.AccountId,
                Amount = p.Amount
            })
            .ToList();
    }

    /// <summary>
    /// Обновляет балансы конечных точек и возвращает идентификаторы затронутых категорий
    /// </summary>
    private async Task<HashSet<Guid>> ApplyBalanceUpdatesAsync(
        List<Posting> postings,
        Dictionary<Guid, TransactionEndpoint> endpointsById,
        CancellationToken token)
    {
        var storageIds = postings
            .Where(p => endpointsById[p.AccountId].EndpointType == EndpointType.Storage)
            .Select(p => p.AccountId)
            .Distinct()
            .ToList();

        var categoryIds = postings
            .Where(p => endpointsById[p.AccountId].EndpointType == EndpointType.Category)
            .Select(p => p.AccountId)
            .Distinct()
            .ToList();

        var storageBalances = await _transactionRepository.GetStorageBalancesAsync(storageIds, token);
        var categoryBalances = await _transactionRepository.GetCategoryBalancesAsync(categoryIds, token);

        var affectedCategoryIds = new HashSet<Guid>();

        foreach (var posting in postings)
        {
            var endpoint = endpointsById[posting.AccountId];

            var currentBalance = endpoint.EndpointType == EndpointType.Storage
                ? storageBalances.TryGetValue(endpoint.Id, out var sb) ? sb : 0m
                : categoryBalances.TryGetValue(endpoint.Id, out var cb) ? cb : 0m;

            endpoint.Value = currentBalance + posting.Amount;
            _transactionEndpointRepository.Update(endpoint);

            if (endpoint.EndpointType == EndpointType.Category)
            {
                affectedCategoryIds.Add(endpoint.Id);
            }
        }

        return affectedCategoryIds;
    }
}
