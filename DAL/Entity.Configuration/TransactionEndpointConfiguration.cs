using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Entity.Configuration;

/// <summary>
/// Конфигурация <see cref="TransactionEndpoint"/>
/// </summary>
public class TransactionEndpointConfiguration : IEntityTypeConfiguration<TransactionEndpoint>
{
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<TransactionEndpoint> builder)
	{
		builder.ToTable(nameof(TransactionEndpoint));

		builder.HasKey(x => x.Id);

		builder.HasOne(x => x.User)
			.WithMany(x => x.TransactionEndpoints)
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
