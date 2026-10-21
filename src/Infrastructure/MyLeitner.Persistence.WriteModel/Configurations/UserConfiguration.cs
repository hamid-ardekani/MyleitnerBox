using Dela.Toolkit.Persistence.Sql;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeitner.Domain.Users;

namespace MyLeitner.Persistence.WriteModel.Configurations;

public class UserConfiguration : BaseEntityTypeConfiguration<User, long>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(User).Pluralize(), MyLeitnerDbContext.DEFAULT_SCHEMA);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => new { u.ExternalProvider, u.ExternalSubject })
            .IsUnique()
            .HasFilter("[ExternalProvider] IS NOT NULL");

        builder.Property(u => u.Email).HasMaxLength(User.EmailMaxLength).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(User.DisplayNameMaxLength).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(500);
        builder.Property(u => u.ExternalProvider).HasMaxLength(50);
        builder.Property(u => u.ExternalSubject).HasMaxLength(200);
        builder.Property(u => u.IsSuperUser).IsRequired().HasDefaultValue(false);

        // کاربر ۱ همان کاربری است که تا حالا همهٔ داده‌ها با UserId=1 به او وصل بوده؛ بدون رمز ساخته می‌شود
        // و اولین ثبت‌نام با همین ایمیل، رمزش را تعیین و حساب را تصاحب می‌کند.
        builder.HasData(new
        {
            Id = 1L,
            Email = "hr.ardekani@gmail.com",
            DisplayName = "hr.ardekani",
            IsSuperUser = true,
            CreatedAt = new DateTime(2026, 10, 11, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
