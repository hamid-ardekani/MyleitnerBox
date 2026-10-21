using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MyLeitner.Persistence.ReadModel.Sql.ReadModels;

namespace MyLeitner.Persistence.ReadModel.Sql.Contexts;

public partial class ReadModelDbContext : DbContext
{
    public ReadModelDbContext()
    {
    }

    public ReadModelDbContext(DbContextOptions<ReadModelDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Box> Boxes { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryBox> CategoryBoxes { get; set; }

    public virtual DbSet<Flashcard> Flashcards { get; set; }

    public virtual DbSet<LearningSchedule> LearningSchedules { get; set; }

    public virtual DbSet<ReviewLog> ReviewLogs { get; set; }

    public virtual DbSet<ReviewGoal> ReviewGoals { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // اگر از طریق DI (AddDbContext) از قبل پیکربندی شده - مثلا با InMemory برای
        // تست‌ها - این‌جا نباید دوباره UseSqlServer را رویش صدا بزنیم، وگرنه EF Core
        // دو provider هم‌زمان می‌بیند و خطا می‌دهد. این فقط برای ساخت مستقیم
        // (new ReadModelDbContext() بدون options) به‌عنوان fallback لازم است.
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer("Name=DefaultConnection");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Persian_100_CI_AI");

        modelBuilder.Entity<Box>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.Name }, "IX_Boxes_UserId_Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<CategoryBox>(entity =>
        {
            entity.HasIndex(e => e.BoxId, "IX_CategoryBoxes_BoxId");

            entity.HasIndex(e => e.CategoryId, "IX_CategoryBoxes_CategoryId");

            entity.HasOne(d => d.Box).WithMany(p => p.CategoryBoxes).HasForeignKey(d => d.BoxId);

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryBoxes).HasForeignKey(d => d.CategoryId);
        });

        modelBuilder.Entity<Flashcard>(entity =>
        {
            entity.HasIndex(e => e.BoxId, "IX_Flashcards_BoxId");

            entity.HasOne(d => d.Box).WithMany(p => p.Flashcards).HasForeignKey(d => d.BoxId);
        });

        modelBuilder.Entity<LearningSchedule>(entity =>
        {
            entity.HasIndex(e => e.FlashcardId, "IX_LearningSchedules_FlashcardId").IsUnique();

            entity.HasOne(d => d.Flashcard).WithOne(p => p.LearningSchedule).HasForeignKey<LearningSchedule>(d => d.FlashcardId);
        });

        modelBuilder.Entity<ReviewLog>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ReviewDate }, "IX_ReviewLogs_UserId_ReviewDate");

            entity.HasIndex(e => new { e.FlashcardId, e.AnsweredAt }, "IX_ReviewLogs_FlashcardId_AnsweredAt");
        });

        modelBuilder.Entity<ReviewGoal>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_ReviewGoals_UserId").IsUnique();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
