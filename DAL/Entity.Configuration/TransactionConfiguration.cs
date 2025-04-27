using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Entity.Configuration;

/// <summary>
/// Конфигурация <see cref="Transaction"/>
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<Transaction> builder)
	{
		builder.ToTable(nameof(Transaction));

		builder.HasKey(x => x.Id);

		builder.HasOne(x => x.User)
			.WithMany(x => x.Transactions)
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.NoAction);
		builder.HasOne(x => x.CashVault)
			.WithMany(x => x.Transactions)
			.HasForeignKey(x => x.FromCashVaultId)
			.OnDelete(DeleteBehavior.NoAction);
		builder.HasOne(x => x.SpendingArea)
			.WithMany(x => x.Transactions)
			.HasForeignKey(x => x.ToSpendingAreaId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
