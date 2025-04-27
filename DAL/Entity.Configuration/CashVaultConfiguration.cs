using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Entity.Configuration;

/// <summary>
/// Конфигурация <see cref="CashVault"/>
/// </summary>
public class CashVaultConfiguration : IEntityTypeConfiguration<CashVault>
{
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<CashVault> builder)
	{
		builder.ToTable(nameof(CashVault));

		builder.HasKey(x => x.Id);

		builder.HasOne(x => x.User)
			.WithMany(x => x.CashVaults)
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
