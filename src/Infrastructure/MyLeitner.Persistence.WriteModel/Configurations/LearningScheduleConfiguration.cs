using Dela.Toolkit.Persistence.Sql;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.LearningSchedules;

namespace MyLeitner.Persistence.WriteModel.Configurations;

public class LearningScheduleConfiguration:BaseEntityTypeConfiguration<LearningSchedule,long>
{
    public override void Configure(EntityTypeBuilder<LearningSchedule> builder)
    { 
        base.Configure(builder);
        
        builder.ToTable(nameof(LearningSchedule).Pluralize(), MyLeitnerDbContext.DEFAULT_SCHEMA);
        
        builder.HasOne<Flashcard>()
            .WithOne() 
            .OnDelete(DeleteBehavior.Cascade);
    }
}