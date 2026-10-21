using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Dtos;
using MyLeitner.Application.Contract.ReadStores;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Application.Learnings.Queries;

public record GetNextPlayFlashcardInBoxQuery(long BoxId, long UserId) : IQuery;

public class GetNextPlayFlashcardInBoxQueryHandler : IQueryHandler<GetNextPlayFlashcardInBoxQuery, CardDto>
{
    private readonly ICardReadRepository _cards;
    private readonly IStudySessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;

    public GetNextPlayFlashcardInBoxQueryHandler(ICardReadRepository cards, IStudySessionRepository sessions, IUnitOfWork unitOfWork)
    {
        _cards = cards;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
    }

    public async Task<CardDto?> Handle(GetNextPlayFlashcardInBoxQuery query, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var today = StudySessionClock.LocalToday(now);

        // session امروز را بگیر (اگر نبود ساخته می‌شود)
        var session = await _sessions.GetAsync(query.UserId, StudySessionScopeType.Box, query.BoxId, today, ct);
        if (session is null)
        {
            session = StudySession.ForAdd(query.UserId, StudySessionScopeType.Box, query.BoxId, today);
            await _sessions.AddAsync(session, ct);
        }

        // کارت‌های کاندید
        var candidates = await _cards.GetDueCardsInBox(query.BoxId, ct);

        if (candidates.Count == 0)
            return null;

        // کارت بعدی را انتخاب کن
        var next = StudySessionCardSelector.SelectNext(
            session,
            candidates.Select(c => new CardCandidate(c.CardId, c.BoxId, c.Difficulty, c.Stability, c.NextReviewAt, c.IsNew, c.AddedToReviewToday)),
            now);

        if (next is null)
            return null;

        // وضعیت session را ذخیره کن
        session.ShowFlashcard(next.CardId);
        await _unitOfWork.CommitAsync(ct);

        // محتوای کارت را برگردان
        return await _cards.GetCardContentAsync(next.CardId, ct);
    }
}