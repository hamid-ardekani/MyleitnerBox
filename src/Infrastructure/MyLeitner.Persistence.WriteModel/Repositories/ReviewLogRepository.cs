using MyLeitner.Domain.ReviewLogs;

namespace MyLeitner.Persistence.WriteModel.Repositories;

public class ReviewLogRepository : IReviewLogRepository
{
    private readonly MyLeitnerDbContext _context;

    public ReviewLogRepository(MyLeitnerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ReviewLog reviewLog, CancellationToken ct)
    {
        await _context.ReviewLogs.AddAsync(reviewLog, ct);
    }
}
