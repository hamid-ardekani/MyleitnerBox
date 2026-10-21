using Dela.Toolkit.Persistence.Sql;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeitner.Domain.ReviewGoals;

namespace MyLeitner.Persistence.WriteModel.Configurations;

public class ReviewGoalConfiguration : BaseEntityTypeConfiguration<ReviewGoal, long>
{
    public override void Configure(EntityTypeBuilder<ReviewGoal> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(ReviewGoal).Pluralize(), MyLeitnerDbContext.DEFAULT_SCHEMA);

        // یک ردیف برای هر کاربر
        builder.HasIndex(g => g.UserId).IsUnique();
    }
}
