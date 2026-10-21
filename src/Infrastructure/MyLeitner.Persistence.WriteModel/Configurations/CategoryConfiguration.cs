using Dela.Toolkit.Persistence.Sql;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeitner.Domain.Categories;

namespace MyLeitner.Persistence.WriteModel.Configurations;

public class CategoryConfiguration: BaseEntityTypeConfiguration<Category, long>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder);
        builder.ToTable(nameof(Category).Pluralize(), MyLeitnerDbContext.DEFAULT_SCHEMA);

        builder
            .Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();
 
    }
}