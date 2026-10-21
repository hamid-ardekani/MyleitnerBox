using Microsoft.EntityFrameworkCore;
using MyLeitner.Domain.ReviewGoals;

namespace MyLeitner.Persistence.WriteModel.Repositories;

public class ReviewGoalRepository(MyLeitnerDbContext context) : IReviewGoalRepository
{
    public Task<ReviewGoal?> GetByUserIdAsync(long userId, CancellationToken ct) =>
        context.ReviewGoals.FirstOrDefaultAsync(g => g.UserId == userId, ct);

    public async Task AddAsync(ReviewGoal goal, CancellationToken ct)
    {
        await context.ReviewGoals.AddAsync(goal, ct);
    }
}
