using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Context.Tests;
using Nasurino.SmartWallet.Entities;
using Xunit;

namespace Nasurino.SmartWallet.Context.Repository.Tests;

/// <summary>
/// Проверка каскадного сохранения постингов при добавлении транзакции.
/// Регрессионный тест на баг: постинги не сохранялись и имели пустой Id
/// из-за того, что IDataStorageContext.Create использовал Entry().State вместо DbSet.Add.
/// </summary>
public class TransactionRepositoryPostingTests : SmartWalletContextInMemory
{
    private readonly TransactionRepository _transactionRepository;

    public TransactionRepositoryPostingTests()
    {
        _transactionRepository = new TransactionRepository(StorageContext);
    }

    [Fact]
    async Task AddTransactionShouldPersistPostingsWithGeneratedId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var transaction = new Transaction
        {
            UserId = userId,
            Type = TransactionType.AdjustmentIncrease,
            Postings = new List<Posting>
            {
                new Posting
                {
                    AccountId = accountId,
                    Amount = 100_000m
                }
            }
        };

        // Act
        _transactionRepository.Add(transaction);
        await Context.SaveChangesAsync();

        // Assert
        transaction.Id.Should().NotBe(Guid.Empty);

        var savedPostings = await Context.Set<Posting>().ToListAsync();
        savedPostings.Should().ContainSingle();
        savedPostings[0].Id.Should().NotBe(Guid.Empty);
        savedPostings[0].AccountId.Should().Be(accountId);
        savedPostings[0].Amount.Should().Be(100_000m);
        savedPostings[0].TransactionId.Should().Be(transaction.Id);
    }
}
