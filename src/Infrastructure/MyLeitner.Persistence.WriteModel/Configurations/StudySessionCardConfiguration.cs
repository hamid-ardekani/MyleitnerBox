using Dela.Toolkit.Persistence.Sql;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Persistence.WriteModel.Configurations;

public class StudySessionCardConfiguration : BaseEntityTypeConfiguration<StudySessionCard, long>
{
    public override void Configure(EntityTypeBuilder<StudySessionCard> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(StudySessionCard).Pluralize(), MyLeitnerDbContext.DEFAULT_SCHEMA);

        // حذف فلش‌کارت (یا جعبه‌اش) سوابق مرور آن را هم پاک می‌کند - وگرنه ردیف یتیم می‌ماند
        builder
            .HasOne<Flashcard>()
            .WithMany()
            .HasForeignKey(c => c.FlashcardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
