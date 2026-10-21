using Dela.Toolkit.Application;
using MyLeitner.Application.Contract.Dtos;
using MyLeitner.Application.Contract.ReadStores;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Application.Learnings.Queries;

/// <param name="CategoryId">مرور دسته‌بندی؛ وگرنه <paramref name="BoxId"/>.</param>
public record GetStudyOverviewQuery(long? BoxId, long? CategoryId, long UserId) : IQuery;

/// <summary>
/// صفحهٔ پیش از مرور و نوارهای «قبل/بعد از پاسخ». چیزی ذخیره نمی‌کند؛ اگر session امروز نبود فقط در حافظه ساخته می‌شود
/// تا «کارت بعدی» همان انتخاب الگوریتم باشد.
/// </summary>
public class GetStudyOverviewQueryHandler(ICardReadRepository cards, IStudySessionRepository sessions)
    : IQueryHandler<GetStudyOverviewQuery, StudyOverviewDto>
{
    private static readonly int[] AllSteps = Enumerable.Range(LearningStep.New, LearningStep.Mastered + 1).ToArray();

    public async Task<StudyOverviewDto> Handle(GetStudyOverviewQuery query, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = StudySessionClock.LocalToday(now);
        var isCategory = query.CategoryId.HasValue;
        var scopeId = query.CategoryId ?? query.BoxId ?? throw new ArgumentException("boxId or categoryId is required");

        var candidates = isCategory
            ? await cards.GetDueCardsInCategory(scopeId, cancellationToken)
            : await cards.GetDueCardsInBox(scopeId, cancellationToken);
        var boxes = await cards.GetScopeBoxesAsync(isCategory, scopeId, cancellationToken);
        var answered = await cards.GetAnsweredTodayAsync(boxes.Select(b => b.BoxId).ToList(), today, cancellationToken);
        var plans = await cards.GetBoxPlansAsync(isCategory, scopeId, cancellationToken);

        var scopeType = isCategory ? StudySessionScopeType.Category : StudySessionScopeType.Box;
        var session = await sessions.GetAsync(query.UserId, scopeType, scopeId, today, cancellationToken)
                      ?? StudySession.ForAdd(query.UserId, scopeType, scopeId, today);

        var next = StudySessionCardSelector.SelectNext(
            session,
            candidates.Select(c => new CardCandidate(c.CardId, c.BoxId, c.Difficulty, c.Stability, c.NextReviewAt, c.IsNew, c.AddedToReviewToday)),
            now);

        var pending = candidates.Select(c => (c.CardId, c.BoxId, Step: LearningStep.For(c.IsNew, c.Interval))).ToList();
        int? nextStep = next is null ? null : pending.First(p => p.CardId == next.CardId).Step;

        var boxDtos = boxes
            .Select(b =>
            {
                var boxPending = pending.Where(p => p.BoxId == b.BoxId).ToList();
                var boxAnswered = answered.Where(a => a.BoxId == b.BoxId).ToList();
                return new StudyOverviewBoxDto(
                    b.BoxId,
                    b.Name,
                    boxPending.Count,
                    boxAnswered.Count,
                    boxAnswered.Sum(a => a.WrongAnswers),
                    CountBySteps(boxPending.Select(p => p.Step)),
                    Box.MaxTodayWrongForNewCards,
                    plans.GetValueOrDefault(b.BoxId)?.NewCards,
                    plans.GetValueOrDefault(b.BoxId)?.Backlog);
            })
            .ToList();

        return new StudyOverviewDto(
            pending.Count,
            CountBySteps(pending.Select(p => p.Step)),
            answered.Count,
            CountBySteps(answered.Select(a => a.Step)),
            boxDtos,
            nextStep);
    }

    private static List<StepCountDto> CountBySteps(IEnumerable<int> steps)
    {
        var counts = steps.GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count());
        return AllSteps.Select(step => new StepCountDto(step, counts.GetValueOrDefault(step))).ToList();
    }
}
