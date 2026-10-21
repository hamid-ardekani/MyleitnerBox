using Dela.Toolkit.Persistence.Sql;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeitner.Domain.Boxes;

namespace MyLeitner.Persistence.WriteModel.Configurations;

public class BoxConfiguration : BaseEntityTypeConfiguration<Box, long>
{
    public override void Configure(EntityTypeBuilder<Box> builder)
    {
        base.Configure(builder);
        builder.ToTable(nameof(Box).Pluralize(), MyLeitnerDbContext.DEFAULT_SCHEMA);
        
        builder.HasIndex(b=> new {b.UserId, b.Name})
            .IsUnique();

        builder
            .Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired(); 
        
        builder
            .Property(c => c.OrderBox) 
            .IsRequired(); 
        
        builder
            .Property(c => c.ReadFlashcardAloud) 
            .IsRequired(); 
    }
}