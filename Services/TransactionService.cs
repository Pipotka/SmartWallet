using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts.Models;
using Nasurino.SmartWallet.BackgroundTaskSystem.Contracts;
using Nasurino.SmartWallet.Services.Contracts.BackgroundService;
using Nasurino.SmartWallet.Entities;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Exceptions;
using Nasurino.SmartWallet.Service.Models;
using Nasurino.SmartWallet.Service.Models.CreateModels;
using Nasurino.SmartWallet.Service.Models.DeleteModels;
using Nasurino.SmartWallet.Service.Models.Models;
using Services.Contracts;

namespace Nasurino.SmartWallet.Services;

public sealed class TransactionService(
    IUnitOfWork unitOfWork,
    ISmartWalletValidateService validateService,
    IMapper mapper,
    IBackgroundTaskSystemProvider backgroundTaskSystemProvider,
    IOptions<ApiSettings> apiSettings) : ITransactionService
{
    private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
    private readonly ITransactionRepository _transactionRepository = unitOfWork.TransactionRepository;
    private readonly ITransactionEndpointRepository _transactionEndpointRepository = unitOfWork.TransactionEndpointRepository;
    private readonly IPostingRepository _postingRepository = unitOfWork.PostingRepository;
    private readonly IBackgroundTaskSystemProvider _backgroundTaskSystemProvider = backgroundTaskSystemProvider;
    private readonly ApiSettings _apiSettings = apiSettings.Value;

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

        var systemIds = (await _transactionEndpointRepository.GetListByUserIdAsync(userId, token))
            .Where(e => e.EndpointType == EndpointType.System)
            .Select(e => e.Id)
            .ToHashSet();

        var result = mapper.Map<PagedResultModel<TransactionModel>>(pagedResult);
        foreach (var item in result.Items)
        {
            item.Postings.RemoveAll(p => systemIds.Contains(p.AccountId));
        }

        return result;
    }

    async Task<TransactionModel> ITransactionService.CreateAsync(CreateTransactionModel model, CancellationToken token)
    {
        await validateService.ValidateAsync(model, token);

        _ = await _userRepository.GetUserByIdAsync(model.UserId, token)
            ?? throw new EntityNotFoundByIdServiceException<User>(model.UserId);

        var maxPostings = Math.Max(2, _apiSettings.PostingSettings.MaxPostingsPerTransaction);

        ValidatePostings(model.Postings, maxPostings);

        var accountIds = model.Postings.Select(p => p.AccountId).ToList();
        var endpoints = await _transactionEndpointRepository.GetListByIdsAndUserIdAsync(model.UserId, accountIds, token);
        var endpointsById = endpoints.ToDictionary(e => e.Id);

        ValidateAccounts(model.Postings, endpointsById);

        var systemEndpoint = await _transactionEndpointRepository.GetByNameAndUserIdAsync(model.UserId, "System", token)
            ?? throw new CodedServiceException("internal_error", "System endpoint not found", StatusCodes.Status500InternalServerError);

        var (type, systemPosting) = ClassifyTransaction(model.Postings, endpointsById, systemEndpoint.Id);

        var transactionId = Guid.NewGuid();
        var transaction = new Transaction
        {
            Id = transactionId,
            UserId = model.UserId,
            Type = type,
            Postings = BuildPostings(model.Postings, systemPosting, transactionId)
        };

        await ApplyBalanceUpdatesAsync(transaction.Postings.ToList(), endpointsById, token);

        _transactionRepository.Add(transaction);
        _postingRepository.AddRange(transaction.Postings);
        await unitOfWork.SaveChangesAsync(token);

        var categoryIds = transaction.Postings
            .Where(p => endpointsById.TryGetValue(p.AccountId, out var e) && e.EndpointType == EndpointType.Category)
            .Select(p => p.AccountId)
            .ToHashSet();

        if (categoryIds.Count > 0)
        {
            var day = DateTime.UtcNow.Date;
            _backgroundTaskSystemProvider.FireAndForget<IDailyExpenseCategorieRecalculationService>(s =>
                s.RecalculateManyAsync(model.UserId, categoryIds, day, token));
        }

        var result = mapper.Map<TransactionModel>(transaction);
        result.Postings.RemoveAll(p => p.AccountId == systemEndpoint.Id);
        return result;
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

            if (account.EndpointType == EndpointType.System)
            {
                posting.DeletedAt = DateTimeOffset.UtcNow;
                _postingRepository.Update(posting);
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

    private static void ValidatePostings(List<CreateTransactionPostingModel> postings, int maxPostings)
    {
        if (postings == null || postings.Count == 0)
        {
            throw new CodedServiceException("POSTINGS_EMPTY", "Список проводок пуст", StatusCodes.Status400BadRequest);
        }

        if (postings.Count > maxPostings)
        {
            throw new CodedServiceException("POSTINGS_LIMIT_EXCEEDED",
                $"Превышен лимит проводок ({maxPostings})",
                StatusCodes.Status400BadRequest);
        }

        var seenAccounts = new HashSet<Guid>();

        foreach (var posting in postings)
        {
            if (posting.AccountId == Guid.Empty)
            {
                throw new CodedServiceException("INVALID_ACCOUNT_ID",
                    "Идентификатор счета не может быть пустым",
                    StatusCodes.Status400BadRequest);
            }

            if (posting.Amount == 0)
            {
                throw new CodedServiceException("ZERO_AMOUNT",
                    "Сумма проводки не может быть равна нулю",
                    StatusCodes.Status400BadRequest);
            }

            if (!seenAccounts.Add(posting.AccountId))
            {
                throw new CodedServiceException("DUPLICATE_ACCOUNT_ID",
                    $"Счет {posting.AccountId} указан более одного раза",
                    StatusCodes.Status400BadRequest);
            }
        }
    }

    private static void ValidateAccounts(
        List<CreateTransactionPostingModel> postings,
        Dictionary<Guid, TransactionEndpoint> endpointsById)
    {
        foreach (var posting in postings)
        {
            if (!endpointsById.TryGetValue(posting.AccountId, out var endpoint))
            {
                throw new CodedServiceException("ACCOUNT_NOT_FOUND",
                    $"Счет {posting.AccountId} не найден",
                    StatusCodes.Status404NotFound);
            }

            if (endpoint.EndpointType == EndpointType.System)
            {
                throw new CodedServiceException("ACCOUNT_NOT_FOUND",
                    $"Счет {posting.AccountId} не найден",
                    StatusCodes.Status404NotFound);
            }
        }
    }

    private static (TransactionType Type, Posting? SystemPosting) ClassifyTransaction(
        List<CreateTransactionPostingModel> postings,
        Dictionary<Guid, TransactionEndpoint> endpointsById,
        Guid systemEndpointId)
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
                return (TransactionType.Expense, null);
            }

            throw new CodedServiceException("INVALID_POSTING_COMBINATION",
                "Комбинация проводок не соответствует ни одному типу транзакции",
                StatusCodes.Status400BadRequest);
        }

        var signs = storagePostings
            .Select(p => Math.Sign(p.Amount))
            .Distinct()
            .ToHashSet();

        if (signs.Count == 2 && userSum == 0)
        {
            return (TransactionType.Transfer, null);
        }

        if (signs.Count == 1 && signs.Contains(1) && userSum > 0)
        {
            return (TransactionType.AdjustmentIncrease, new Posting
            {
                AccountId = systemEndpointId,
                Amount = -userSum
            });
        }

        if (signs.Count == 1 && signs.Contains(-1) && userSum < 0)
        {
            return (TransactionType.AdjustmentDecrease, new Posting
            {
                AccountId = systemEndpointId,
                Amount = -userSum
            });
        }

        throw new CodedServiceException("INVALID_POSTING_COMBINATION",
            "Комбинация проводок не соответствует ни одному типу транзакции",
            StatusCodes.Status400BadRequest);
    }

    private static List<Posting> BuildPostings(
        List<CreateTransactionPostingModel> userPostings,
        Posting? systemPosting,
        Guid transactionId)
    {
        var postings = userPostings
            .Select(p => new Posting
            {
                Id = Guid.NewGuid(),
                TransactionId = transactionId,
                AccountId = p.AccountId,
                Amount = p.Amount
            })
            .ToList();

        if (systemPosting != null)
        {
            systemPosting.Id = Guid.NewGuid();
            systemPosting.TransactionId = transactionId;
            postings.Add(systemPosting);
        }

        return postings;
    }

    private async Task ApplyBalanceUpdatesAsync(
        List<Posting> postings,
        Dictionary<Guid, TransactionEndpoint> endpointsById,
        CancellationToken token)
    {
        var nonSystemPostings = postings
            .Where(p => endpointsById.TryGetValue(p.AccountId, out var e) && e.EndpointType != EndpointType.System)
            .ToList();

        var storageIds = nonSystemPostings
            .Where(p => endpointsById[p.AccountId].EndpointType == EndpointType.Storage)
            .Select(p => p.AccountId)
            .Distinct()
            .ToList();

        var categoryIds = nonSystemPostings
            .Where(p => endpointsById[p.AccountId].EndpointType == EndpointType.Category)
            .Select(p => p.AccountId)
            .Distinct()
            .ToList();

        var storageBalances = await _transactionRepository.GetStorageBalancesAsync(storageIds, token);
        var categoryBalances = await _transactionRepository.GetCategoryBalancesAsync(categoryIds, token);

        foreach (var posting in nonSystemPostings)
        {
            var endpoint = endpointsById[posting.AccountId];

            var currentBalance = endpoint.EndpointType == EndpointType.Storage
                ? storageBalances.TryGetValue(endpoint.Id, out var sb) ? sb : 0m
                : categoryBalances.TryGetValue(endpoint.Id, out var cb) ? cb : 0m;

            endpoint.Value = currentBalance + posting.Amount;
            _transactionEndpointRepository.Update(endpoint);
        }
    }
}
