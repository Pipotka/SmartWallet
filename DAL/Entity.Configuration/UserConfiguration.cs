using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Entity.Configuration;

/// <summary>
/// Конфигурация <see cref="User"/>
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable(nameof(User));

		builder.HasKey(x => x.Id);
	}
}