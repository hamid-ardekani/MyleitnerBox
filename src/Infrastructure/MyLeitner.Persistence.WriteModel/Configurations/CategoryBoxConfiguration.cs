using Dela.Toolkit.Persistence.Sql;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Categories;

namespace MyLeitner.Persistence.WriteModel.Configurations;

public class CategoryBoxConfiguration: BaseEntityTypeConfiguration<CategoryBox, long>
{
    public override void Configure(EntityTypeBuilder<CategoryBox> builder)
    {
        base.Configure(builder);
        builder.ToTable(nameof(CategoryBox).Pluralize(), MyLeitnerDbContext.DEFAULT_SCHEMA);
           
        builder
            .HasOne<Category>()
            .WithMany(c=>c.CategoryBoxes)
            .HasForeignKey(sc => sc.CategoryId);
        
        
        builder
            .HasOne<Box>()
            .WithMany()
            .HasForeignKey(sc => sc.BoxId);

        // دفاع در عمق در برابر وصل شدن دوبارهٔ یک جعبه به یک دسته (قبل از اینکه GetByIdAsync شامل CategoryBoxes شود ممکن بود - CategoryRepository را ببین)
        builder
            .HasIndex(sc => new { sc.CategoryId, sc.BoxId })
            .IsUnique();
    }
}