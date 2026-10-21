using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Dtos;
using MyLeitner.Application.Contract.ReadStores;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Application.Learnings.Queries;

public sealed record GetNextCardInCategoryQuery(long CategoryId, long UserId) : IQuery;

public sealed class GetNextCardInCategoryQueryHandler : IQueryHandler<GetNextCardInCategoryQuery, CardDto>
{
    private readonly ICardReadRepository _cards;
    private readonly IStudySessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;

    public GetNextCardInCategoryQueryHandler(ICardReadRepository cards, IStudySessionRepository sessions, IUnitOfWork unitOfWork)
    {
        _cards = cards;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
    }

    public async Task<CardDto?> Handle(GetNextCardInCategoryQuery query, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var today = StudySessionClock.LocalToday(now);

        // session امروز را بگیر (اگر نبود ساخته می‌شود)
        var session = await _sessions.GetAsync(query.UserId, StudySessionScopeType.Category, query.CategoryId, today, ct);
        if (session is null)
        {
            session = StudySession.ForAdd(query.UserId, StudySessionScopeType.Category, query.CategoryId, today);
            await _sessions.AddAsync(session, ct);
        }

        // کارت‌های کاندید
        var candidates = await _cards.GetDueCardsInCategory(query.CategoryId, ct);

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
