using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Entity.Configuration;

/// <summary>
/// Конфигурация <see cref="RefreshToken"/>
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<RefreshToken> builder)
	{
		builder.ToTable(nameof(RefreshToken));

		builder.HasKey(x => x.Id);

		builder.HasIndex(x => x.Token).IsUnique();

		builder.HasIndex(x => x.UserId);

		builder.Property(x => x.Token).IsRequired();

		builder.HasOne(x => x.User)
			.WithMany(x => x.RefreshTokens)
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
