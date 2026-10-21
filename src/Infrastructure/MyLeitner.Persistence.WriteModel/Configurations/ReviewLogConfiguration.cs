using Dela.Toolkit.Persistence.Sql;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeitner.Domain.ReviewLogs;

namespace MyLeitner.Persistence.WriteModel.Configurations;

public class ReviewLogConfiguration : BaseEntityTypeConfiguration<ReviewLog, long>
{
    public override void Configure(EntityTypeBuilder<ReviewLog> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(ReviewLog).Pluralize(), MyLeitnerDbContext.DEFAULT_SCHEMA);

        // عمداً بدون کلید خارجی: تاریخچه با حذف فلش‌کارت/جعبه/دسته‌بندی نباید پاک شود

        // گزارش‌های روزانهٔ هر کاربر
        builder.HasIndex(r => new { r.UserId, r.ReviewDate });

        // تاریخچهٔ یک کارت به ترتیب زمان
        builder.HasIndex(r => new { r.FlashcardId, r.AnsweredAt });
    }
}
