using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Nasurino.SmartWallet.Context.Tests;
using Nasurino.SmartWallet.Entities;
using Xunit;

namespace Nasurino.SmartWallet.Context.Repository.Tests;

/// <summary>
/// Проверка мягкого удаления постингов при удалении транзакции.
/// Регрессионный тест: постинги должны остаться в БД с DeletedAt != null
/// (физического удаления нет — пересчёт балансов читает постинги, включая мягко удалённые).
/// По подходу постинги обновляются явно через PostingRepository.UpdateRange, а не каскадом.
/// </summary>
public class TransactionRepositoryPostingDeleteTests : SmartWalletContextInMemory
{
	private readonly TransactionRepository _transactionRepository;
	private readonly PostingRepository _postingRepository;

	public TransactionRepositoryPostingDeleteTests()
	{
		_transactionRepository = new TransactionRepository(StorageContext);
		_postingRepository = new PostingRepository(StorageContext);
	}

	[Fact]
	async Task DeleteTransactionShouldSoftDeletePostingsWithDeletedAt()
	{
		// Arrange — создаём транзакцию с постингом (как в TransactionService.CreateAsync)
		var userId = Guid.NewGuid();
		var accountId = Guid.NewGuid();
		var transactionId = Guid.NewGuid();

		var transaction = new Transaction
		{
			Id = transactionId,
			UserId = userId,
			Type = TransactionType.AdjustmentIncrease
		};

		var postings = new List<Posting>
		{
			new Posting
			{
				TransactionId = transactionId,
				AccountId = accountId,
				Amount = 100_000m
			}
		};

		_transactionRepository.Add(transaction);
		_postingRepository.AddRange(postings);
		await Context.SaveChangesAsync();

		// Act — имитация TransactionService.DeleteAsync на уже загруженной транзакции
		foreach (var posting in postings)
		{
			posting.DeletedAt = DateTimeOffset.UtcNow;
		}

		_postingRepository.UpdateRange(postings);
		_transactionRepository.Delete(transaction);
		await Context.SaveChangesAsync();

		// Assert — постинги остались в БД, помеченные DeletedAt
		var savedPostings = await Context.Set<Posting>().AsNoTracking().ToListAsync();
		savedPostings.Should().ContainSingle();
		savedPostings[0].DeletedAt.Should().NotBeNull();
		savedPostings[0].TransactionId.Should().Be(transactionId);
	}
}
