using Microsoft.EntityFrameworkCore;
using MyLeitner.Domain.LearningSchedules;

namespace MyLeitner.Persistence.WriteModel.Repositories;

public class LearningScheduleRepository : ILearningScheduleRepository
{
    private readonly MyLeitnerDbContext _context;

    public LearningScheduleRepository(MyLeitnerDbContext context)
    {
        _context = context;
    }

    public Task<LearningSchedule?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _context.LearningSchedules.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public Task<LearningSchedule?> GetByFlashcardIdAsync(long flashcardId, CancellationToken cancellationToken)
    {
        return _context.LearningSchedules.FirstOrDefaultAsync(l => l.FlashcardId == flashcardId, cancellationToken);
    }

    public Task AddAsync(LearningSchedule learningSchedule)
    {
        _context.LearningSchedules.AddAsync(learningSchedule);
        return Task.CompletedTask;
    }
}