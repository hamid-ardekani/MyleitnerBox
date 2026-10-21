namespace MyLeitner.Domain.StudySessions;

public interface IStudySessionRepository
{
    Task<StudySession?> GetAsync(long userId, StudySessionScopeType scopeType, long scopeId, DateOnly sessionDate, CancellationToken ct);
    Task AddAsync(StudySession session, CancellationToken ct);

    /// <summary>
    /// sessionهای یک جعبه یا دسته‌بندی را برای حذف علامت می‌زند. ScopeId کلید خارجی ندارد (گاهی جعبه است، گاهی دسته)،
    /// پس حذف جعبه/دسته باید خودش این را صدا بزند.
    /// </summary>
    Task RemoveAllForScopeAsync(StudySessionScopeType scopeType, long scopeId, CancellationToken ct);
}