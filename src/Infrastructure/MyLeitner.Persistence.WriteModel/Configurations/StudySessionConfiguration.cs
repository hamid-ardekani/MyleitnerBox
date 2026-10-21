using Dela.Toolkit.Persistence.Sql;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Persistence.WriteModel.Configurations;

public class StudySessionConfiguration : BaseEntityTypeConfiguration<StudySession, long>
{
    public override void Configure(EntityTypeBuilder<StudySession> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(StudySession).Pluralize(), MyLeitnerDbContext.DEFAULT_SCHEMA);

        // یک session به‌ازای هر (کاربر، دامنه، تاریخ محلی) - رسیدن نیمه‌شب یعنی ردیف جدید
        builder
            .HasIndex(s => new { s.UserId, s.ScopeType, s.ScopeId, s.SessionDate })
            .IsUnique();

        builder
            .HasMany(s => s.Cards)
            .WithOne()
            .HasForeignKey(c => c.StudySessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(s => s.Cards)
            .HasField("_cards")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
