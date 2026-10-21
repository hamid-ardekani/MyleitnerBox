using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Dtos;
using MyLeitner.Application.Contract.ReadStores;
using MyLeitner.Common.Results;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Application.Learnings.Queries;

public record GetNextPlayFlashcardInCategoryQuery(long CategoryId, long UserId) : IQuery;

public class GetNextPlayFlashcardInCategoryQueryHandler : IQueryHandler<GetNextPlayFlashcardInCategoryQuery, Result<StudySessionResult>>
{
    private readonly ICardReadRepository _cards;
    private readonly IStudySessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;

    public GetNextPlayFlashcardInCategoryQueryHandler(ICardReadRepository cards, IStudySessionRepository sessions, IUnitOfWork unitOfWork)
    {
        _cards = cards;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StudySessionResult>> Handle(GetNextPlayFlashcardInCategoryQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = StudySessionClock.LocalToday(now);

            // session امروز را بگیر (اگر نبود ساخته می‌شود)
            var session = await _sessions.GetAsync(query.UserId, StudySessionScopeType.Category, query.CategoryId, today, cancellationToken);
            if (session is null)
            {
                session = StudySession.ForAdd(query.UserId, StudySessionScopeType.Category, query.CategoryId, today);
                await _sessions.AddAsync(session, cancellationToken);
            }

            // کارت‌های کاندید
            var candidates = await _cards.GetDueCardsInCategory(query.CategoryId, cancellationToken);

            // کارت بعدی را انتخاب کن
            var next = StudySessionCardSelector.SelectNext(
                session,
                candidates.Select(c => new CardCandidate(c.CardId, c.BoxId, c.Difficulty, c.Stability, c.NextReviewAt, c.IsNew, c.AddedToReviewToday)),
                now);

            if (next is null)
                return new StudySessionResult(LearningScheduleStatus.AllFlashcardsCompletedForToday, null);

            // وضعیت session را ذخیره کن
            session.ShowFlashcard(next.CardId);
            await _unitOfWork.CommitAsync(cancellationToken);

            // محتوای کارت را برگردان
            var card = await _cards.GetCardContentAsync(next.CardId, cancellationToken);
            var flashcard = card is null ? null : new LearningFlashcardDto(card.CardId, card.BoxId, card.Front, card.Back, card.Step, card.DaysLate, card.BoxName);

            return new StudySessionResult(LearningScheduleStatus.HasFlashcards, flashcard);
        }
        catch (Exception)
        {
            return Error.UnhandledError();
        }
    }
}