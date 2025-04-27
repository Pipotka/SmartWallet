using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Entity.Configuration;

/// <summary>
/// Конфигурация <see cref="SpendingArea"/>
/// </summary>
public class SpendingAreaConfiguration : IEntityTypeConfiguration<SpendingArea>
{
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<SpendingArea> builder)
	{
		builder.ToTable(nameof(SpendingArea));

		builder.HasKey(x => x.Id);

		builder.HasOne(x => x.User)
			.WithMany(x => x.SpendingAreas)
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
