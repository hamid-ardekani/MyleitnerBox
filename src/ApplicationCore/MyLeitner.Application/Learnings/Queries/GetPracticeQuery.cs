using Dela.Toolkit.Application;
using MyLeitner.Application.Contract.Dtos;
using MyLeitner.Application.Contract.ReadStores;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Application.Learnings.Queries;

/// <param name="CategoryId">دور دوم دسته‌بندی؛ وگرنه <paramref name="BoxId"/>.</param>
public record GetPracticeQuery(long? BoxId, long? CategoryId, long UserId) : IQuery;

/// <summary>
/// «دور دوم»: کارت‌هایی از جعبه(های) این مرور که آخرین جواب امروزشان «بلد نبودم» یا «شانسی» بوده و هنوز در مرورند.
/// هر کارت یک بار؛ فقط تمرین است و چیزی ثبت نمی‌شود. روز سخت (بیش از <see cref="HardDayWrongs"/> «بلد نبودم») ۵ کارت، وگرنه حداکثر ۱۰.
/// </summary>
public class GetPracticeQueryHandler(ICardReadRepository cards) : IQueryHandler<GetPracticeQuery, PracticeDto>
{
    public const int HardDayWrongs = 5;
    public const int MaxCards = 10;
    public const int MaxCardsHardDay = 5;

    public async Task<PracticeDto> Handle(GetPracticeQuery query, CancellationToken cancellationToken)
    {
        var today = StudySessionClock.LocalToday(DateTime.UtcNow);
        var isCategory = query.CategoryId.HasValue;
        var scopeId = query.CategoryId ?? query.BoxId ?? throw new ArgumentException("boxId or categoryId is required");

        var boxIds = (await cards.GetScopeBoxesAsync(isCategory, scopeId, cancellationToken)).Select(b => b.BoxId).ToList();
        var weak = (await cards.GetWeakAnswersTodayAsync(query.UserId, boxIds, today, cancellationToken))
            .ToDictionary(w => w.FlashcardId, w => w.AnswerTypeId);

        if (weak.Count == 0)
            return new PracticeDto(0, 0, false, []);

        var wrong = AnswerTypeEnum.Wrong.Id;
        var content = await cards.GetPracticeCardsAsync(weak.Keys.ToList(), cancellationToken);
        var wrongToday = weak.Values.Count(a => a == wrong);
        var hardDay = wrongToday > HardDayWrongs;

        var items = content
            .OrderByDescending(c => weak[c.CardId] == wrong)
            .ThenByDescending(c => c.Difficulty)
            .Take(hardDay ? MaxCardsHardDay : MaxCards)
            .Select(c => new PracticeCardDto(c.CardId, c.BoxId, c.BoxName, c.Front, c.Back, weak[c.CardId]))
            .ToList();

        return new PracticeDto(content.Count, wrongToday, hardDay, items);
    }
}

public record GetPracticeCountsQuery(long UserId) : IQuery;

/// <summary>برای دکمهٔ «دور دوم · X»: تعداد کارت‌های «بلد نبودم»/«شانسی» امروز در هر جعبه/دسته (فقط غیرصفرها).</summary>
public class GetPracticeCountsQueryHandler(ICardReadRepository cards) : IQueryHandler<GetPracticeCountsQuery, PracticeCountsDto>
{
    public async Task<PracticeCountsDto> Handle(GetPracticeCountsQuery query, CancellationToken cancellationToken)
    {
        var today = StudySessionClock.LocalToday(DateTime.UtcNow);
        var weak = await cards.GetWeakAnswersTodayAsync(query.UserId, null, today, cancellationToken);
        if (weak.Count == 0)
            return new PracticeCountsDto([], []);

        var boxes = weak.GroupBy(w => w.BoxId).ToDictionary(g => g.Key, g => g.Count());
        var categoryBoxes = await cards.GetCategoryBoxesAsync(query.UserId, cancellationToken);
        var categories = categoryBoxes
            .Where(cb => boxes.ContainsKey(cb.BoxId))
            .GroupBy(cb => cb.CategoryId)
            .ToDictionary(g => g.Key, g => g.Select(cb => cb.BoxId).Distinct().Sum(id => boxes[id]));

        return new PracticeCountsDto(boxes, categories);
    }
}
