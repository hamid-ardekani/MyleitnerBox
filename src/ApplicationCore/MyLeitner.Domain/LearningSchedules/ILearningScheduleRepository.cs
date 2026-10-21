namespace MyLeitner.Domain.LearningSchedules;

public interface ILearningScheduleRepository
{
    Task<LearningSchedule?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<LearningSchedule?> GetByFlashcardIdAsync(long flashcardId, CancellationToken cancellationToken);
    Task AddAsync(LearningSchedule learningSchedule);
}