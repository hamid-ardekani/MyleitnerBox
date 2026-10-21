 
using Dela.Toolkit.Persistence.Sql;
using Microsoft.EntityFrameworkCore;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Categories; 
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.ReviewGoals;
using MyLeitner.Domain.ReviewLogs;
using MyLeitner.Domain.StudySessions;
using MyLeitner.Domain.Users;
using MyLeitner.Persistence.WriteModel.Configurations;

namespace MyLeitner.Persistence.WriteModel;

public class MyLeitnerDbContext : DbContextBase
{
    public const string DEFAULT_SCHEMA = "dbo";

    public MyLeitnerDbContext(DbContextOptions<MyLeitnerDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; init; } = null!;
    public virtual DbSet<Box> Boxes { get; init; } = null!;
    public virtual DbSet<Domain.Flashcards.Flashcard> Flashcards { get; init; } = null!;
    public virtual DbSet<LearningSchedule> LearningSchedules { get; init; } = null!;
    public virtual DbSet<StudySession> StudySessions { get; init; } = null!;
    public virtual DbSet<ReviewLog> ReviewLogs { get; init; } = null!;
    public virtual DbSet<ReviewGoal> ReviewGoals { get; init; } = null!;
    public virtual DbSet<User> Users { get; init; } = null!;
    public virtual DbSet<RefreshToken> RefreshTokens { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DEFAULT_SCHEMA);
        modelBuilder.UseCollation("Persian_100_CI_AI");

        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryBoxConfiguration());
        modelBuilder.ApplyConfiguration(new BoxConfiguration());
        modelBuilder.ApplyConfiguration(new FlashcardConfiguration());
        modelBuilder.ApplyConfiguration(new LearningScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new StudySessionConfiguration());
        modelBuilder.ApplyConfiguration(new StudySessionCardConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewLogConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewGoalConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}