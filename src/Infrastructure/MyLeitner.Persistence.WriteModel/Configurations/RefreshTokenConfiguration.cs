using Dela.Toolkit.Persistence.Sql;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeitner.Domain.Users;

namespace MyLeitner.Persistence.WriteModel.Configurations;

public class RefreshTokenConfiguration : BaseEntityTypeConfiguration<RefreshToken, long>
{
    public override void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(RefreshToken).Pluralize(), MyLeitnerDbContext.DEFAULT_SCHEMA);

        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.FamilyId);
        builder.HasIndex(t => t.UserId);

        builder.Property(t => t.TokenHash).HasMaxLength(100).IsRequired();
    }
}
