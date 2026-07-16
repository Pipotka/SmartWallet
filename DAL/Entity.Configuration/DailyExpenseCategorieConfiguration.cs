using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nasurino.SmartWallet.Entities;

namespace Nasurino.SmartWallet.Entity.Configuration;

/// <summary>
/// Конфигурация <see cref="DailyExpenseCategorie"/>
/// </summary>
public class DailyExpenseCategorieConfiguration : IEntityTypeConfiguration<DailyExpenseCategorie>
{
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<DailyExpenseCategorie> builder)
	{
		builder.ToTable(nameof(DailyExpenseCategorie));

		builder.HasKey(x => new { x.CategorieId, x.Day });

		builder.HasOne(x => x.Category)
			.WithMany(x => x.DailyExpenseCategories)
			.HasForeignKey(x => x.CategorieId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
