using Dela.Toolkit.Persistence.Sql;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeitner.Domain.Boxes;
using Flashcard = MyLeitner.Domain.Flashcards.Flashcard;

namespace MyLeitner.Persistence.WriteModel.Configurations;

public class FlashcardConfiguration:BaseEntityTypeConfiguration<Flashcard,long>
{
    public override void Configure(EntityTypeBuilder<Flashcard> builder)
    { 
        base.Configure(builder);
         
        builder.ToTable(nameof(Flashcard).Pluralize(), MyLeitnerDbContext.DEFAULT_SCHEMA);
        
        builder.Property(j => j.Order).IsRequired();

        builder.Property(j => j.FrontSearchText).IsRequired().HasMaxLength(4000);
        builder.Property(j => j.BackSearchText).IsRequired().HasMaxLength(4000);

        builder.HasOne<Box>()
            .WithMany()
            .HasForeignKey(d => d.BoxId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .Property(c => c.Tags)
            .IsRequired();
    
    }
}