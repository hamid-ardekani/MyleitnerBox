namespace MyLeitner.Domain.ReviewGoals;

public interface IReviewGoalRepository
{
    Task<ReviewGoal?> GetByUserIdAsync(long userId, CancellationToken ct);
    Task AddAsync(ReviewGoal goal, CancellationToken ct);
}
