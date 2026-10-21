using Microsoft.EntityFrameworkCore;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Persistence.WriteModel.Repositories;

public class StudySessionRepository : IStudySessionRepository
{
    private readonly MyLeitnerDbContext _context;

    public StudySessionRepository(MyLeitnerDbContext context)
    {
        _context = context;
    }

    public Task<StudySession?> GetAsync(long userId, StudySessionScopeType scopeType, long scopeId, DateOnly sessionDate, CancellationToken ct)
    {
        return _context.StudySessions
            .Include(s => s.Cards)
            .FirstOrDefaultAsync(s =>
                s.UserId == userId &&
                s.ScopeType == scopeType &&
                s.ScopeId == scopeId &&
                s.SessionDate == sessionDate, ct);
    }

    public async Task AddAsync(StudySession session, CancellationToken ct)
    {
        await _context.StudySessions.AddAsync(session, ct);
    }

    public async Task RemoveAllForScopeAsync(StudySessionScopeType scopeType, long scopeId, CancellationToken ct)
    {
        // StudySessionCardها با cascade دیتابیس پاک می‌شوند
        var sessions = await _context.StudySessions
            .Where(s => s.ScopeType == scopeType && s.ScopeId == scopeId)
            .ToListAsync(ct);

        _context.StudySessions.RemoveRange(sessions);
    }
}
