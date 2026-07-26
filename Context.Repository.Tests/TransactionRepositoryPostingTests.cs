using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Context.Tests;
using Nasurino.SmartWallet.Entities;
using Xunit;

namespace Nasurino.SmartWallet.Context.Repository.Tests;

/// <summary>
/// Проверка явного сохранения постингов через PostingRepository при добавлении транзакции.
/// Регрессионный тест на баг: постинги не сохранялись и имели пустой Id.
/// Подход: транзакция добавляется через TransactionRepository, постинги — через
/// PostingRepository.AddRange (IDataStorageContext.Create использует Entry().State = Added,
/// поэтому каскадной вставки дочерних сущностей не происходит).
/// </summary>
public class TransactionRepositoryPostingTests : SmartWalletContextInMemory
{
	private readonly TransactionRepository _transactionRepository;
	private readonly PostingRepository _postingRepository;

	public TransactionRepositoryPostingTests()
	{
		_transactionRepository = new TransactionRepository(StorageContext);
		_postingRepository = new PostingRepository(StorageContext);
	}

	[Fact]
	async Task AddTransactionThenAddPostingsViaRepositoryShouldPersistPostingsWithGeneratedId()
	{
		// Arrange
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

		// Act — транзакция и постинги добавляются явно через свои репозитории
		_transactionRepository.Add(transaction);
		_postingRepository.AddRange(postings);
		await Context.SaveChangesAsync();

		// Assert
		transaction.Id.Should().Be(transactionId);

		var savedPostings = await Context.Set<Posting>().ToListAsync();
		savedPostings.Should().ContainSingle();
		savedPostings[0].Id.Should().NotBe(Guid.Empty);
		savedPostings[0].AccountId.Should().Be(accountId);
		savedPostings[0].Amount.Should().Be(100_000m);
		savedPostings[0].TransactionId.Should().Be(transactionId);
	}
}
