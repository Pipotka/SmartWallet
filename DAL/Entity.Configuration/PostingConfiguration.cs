using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Entity.Configuration;

/// <summary>
/// Конфигурация <see cref="Posting"/>
/// </summary>
public class PostingConfiguration : IEntityTypeConfiguration<Posting>
{
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<Posting> builder)
	{
		builder.ToTable(nameof(Posting));

		builder.HasKey(x => x.Id);

		builder.HasOne(x => x.Account)
			.WithMany(x => x.Postings)
			.HasForeignKey(x => x.AccountId)
			.OnDelete(DeleteBehavior.NoAction);

		builder.HasOne(x => x.Transaction)
			.WithMany(x => x.Postings)
			.HasForeignKey(x => x.TransactionId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
