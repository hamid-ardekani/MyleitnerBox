using Microsoft.EntityFrameworkCore;
using MyLeitner.Application.Contract.Dtos;
using MyLeitner.Application.Contract.ReadModels;
using MyLeitner.Application.Contract.ReadStores;
using AnswerTypeEnum = MyLeitner.Domain.Flashcards.AnswerTypeEnum;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.StudySessions;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;
using MyLeitner.Persistence.ReadModel.Sql.ReadModels;

namespace MyLeitner.Persistence.ReadModel.Sql.Service;

public class CardReadRepository(ReadModelDbContext context) : ReadService(context), ICardReadRepository
{
    // یادگرفته‌شده و بایگانی‌شده در مرور نمی‌آیند
    private const int LearningOnly = (int)LearningStatus.Learning;

    public Task<IReadOnlyList<CardReadModel>> GetDueCardsInCategory(long categoryId, CancellationToken ct)
    {
        return GetDueCardsAsync(
            Context.Flashcards.Where(f => f.Box.CategoryBoxes.Any(cb => cb.CategoryId == categoryId)),
            Context.Boxes.Where(b => b.CategoryBoxes.Any(cb => cb.CategoryId == categoryId)),
            ct);
    }

    public Task<IReadOnlyList<CardReadModel>> GetDueCardsInBox(long boxId, CancellationToken ct)
    {
        return GetDueCardsAsync(
            Context.Flashcards.Where(f => f.BoxId == boxId),
            Context.Boxes.Where(b => b.Id == boxId),
            ct);
    }

    /// <summary>
    /// مرورهایی که موعدشان <b>امروز</b> (محلی) است، همه؛ + از «بدهی» هر جعبه فقط سهم آزادشدهٔ امروز
    /// (<see cref="MyLeitner.Domain.Boxes.BacklogQuota"/>)، عقب‌افتاده‌ترین اول؛ + از کارت‌های جدید هر جعبه فقط سهم امروز، به ترتیب خودِ جعبه (Order).
    /// «موعد» روزانه است، نه ساعت دقیق.
    /// </summary>
    private async Task<IReadOnlyList<CardReadModel>> GetDueCardsAsync(IQueryable<Flashcard> flashcards, IQueryable<Box> boxes, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var today = StudySessionClock.LocalToday(now);
        var todayStart = StudySessionClock.UtcStartOf(today);
        var tomorrowStart = StudySessionClock.UtcStartOf(today.AddDays(1));

        var reviews = await flashcards
            .Where(f => f.LearningSchedule != null && f.LearningSchedule.Status == LearningOnly
                        && f.LearningSchedule.NextAnswerDate != null && f.LearningSchedule.NextAnswerDate < tomorrowStart)
            .Select(f => new CardReadModel
            {
                CardId = f.Id,
                BoxId = f.BoxId,
                NextReviewAt = f.LearningSchedule!.NextAnswerDate!.Value,
                Stability = f.LearningSchedule!.Stability,
                Difficulty = f.LearningSchedule!.Difficulty,
                Interval = f.LearningSchedule!.Interval,
                IsNew = false,
            })
            .ToListAsync(ct);

        var allowance = await NewCardAllowance.ForBoxesAsync(Context, boxes, now, ct);

        // بدهی: فعلاً فقط سهم آزادشدهٔ هر جعبه، عقب‌افتاده‌ترین اول
        var dueToday = reviews.Where(r => r.NextReviewAt >= todayStart);
        var backlog = reviews
            .Where(r => r.NextReviewAt < todayStart)
            .OrderBy(r => r.NextReviewAt)
            .GroupBy(r => r.BoxId)
            .SelectMany(g => g.Take(allowance.TryGetValue(g.Key, out var plan) ? plan.Backlog.Available : 0));

        var newRows = await flashcards
            .Where(f => f.LearningSchedule != null && f.LearningSchedule.Status == LearningOnly && f.LearningSchedule.NextAnswerDate == null)
            .OrderBy(f => f.Order).ThenBy(f => f.Id)
            .Select(f => new
            {
                f.Id,
                f.BoxId,
                f.LearningSchedule!.Stability,
                f.LearningSchedule.Difficulty,
                f.LearningSchedule.AddedToReviewOn,
            })
            .ToListAsync(ct);

        var newCards = newRows
            .Select(f => new CardReadModel
            {
                CardId = f.Id,
                BoxId = f.BoxId,
                NextReviewAt = now,
                Stability = f.Stability,
                Difficulty = f.Difficulty,
                Interval = 0,
                IsNew = true,
                AddedToReviewToday = f.AddedToReviewOn >= todayStart && f.AddedToReviewOn < tomorrowStart,
            })
            .ToList();

        // کارت‌هایی که کاربر دستی اضافه کرده همیشه می‌آیند (و اول از سهم جعبه خرج می‌شوند)؛ بقیه باقی‌ماندهٔ سهم را پر می‌کنند
        var admittedNew = newCards
            .GroupBy(c => c.BoxId)
            .SelectMany(g =>
            {
                var allowed = allowance.TryGetValue(g.Key, out var box) ? box.Allowed : 0;
                var added = g.Where(c => c.AddedToReviewToday).ToList();
                return added.Concat(g.Where(c => !c.AddedToReviewToday).Take(Math.Max(0, allowed - added.Count)));
            });

        return dueToday.Concat(backlog).Concat(admittedNew).ToList();
    }

    /// <summary>متن کارت + گامش، چند روز از موعدش گذشته، و نام جعبه - برای نوار بالای کارت در مرور.</summary>
    public async Task<CardDto?> GetCardContentAsync(long flashcardId, CancellationToken ct)
    {
        var card = await Context.Flashcards
            .Where(f => f.Id == flashcardId)
            .Select(f => new
            {
                f.Id,
                f.BoxId,
                BoxName = f.Box.Name,
                f.Front,
                f.Back,
                NextAnswerDate = f.LearningSchedule != null ? f.LearningSchedule.NextAnswerDate : null,
                Interval = f.LearningSchedule != null ? f.LearningSchedule.Interval : 0,
            })
            .FirstOrDefaultAsync(ct);

        if (card == null)
            return null;

        var isNew = card.NextAnswerDate == null;
        int? daysLate = isNew
            ? null
            : Math.Max(0, StudySessionClock.LocalToday(DateTime.UtcNow).DayNumber - StudySessionClock.LocalToday(card.NextAnswerDate!.Value).DayNumber);

        return new CardDto(card.Id, card.BoxId, card.Front, card.Back, LearningStep.For(isNew, card.Interval), daysLate, card.BoxName);
    }

    public async Task<IReadOnlyList<PracticeCardReadModel>> GetPracticeCardsAsync(IReadOnlyCollection<long> flashcardIds, CancellationToken ct)
    {
        return await Context.Flashcards
            .Where(f => flashcardIds.Contains(f.Id) && f.LearningSchedule != null && f.LearningSchedule.Status == LearningOnly)
            .Select(f => new PracticeCardReadModel(f.Id, f.BoxId, f.Box.Name, f.Front, f.Back, f.LearningSchedule!.Difficulty))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WeakAnswerReadModel>> GetWeakAnswersTodayAsync(long userId, IReadOnlyCollection<long>? boxIds, DateOnly today, CancellationToken ct)
    {
        var logs = Context.ReviewLogs.Where(l => l.UserId == userId && l.ReviewDate == today);
        if (boxIds != null)
            logs = logs.Where(l => boxIds.Contains(l.BoxId));

        var rows = await logs.Select(l => new { l.FlashcardId, l.BoxId, l.AnswerTypeId, l.AnsweredAt }).ToListAsync(ct);

        // آخرین جواب امروزِ هر کارت تعیین‌کننده است (کارت روزی یک بار جواب می‌گیرد، ولی احتیاط)
        var wrong = AnswerTypeEnum.Wrong.Id;
        var guess = AnswerTypeEnum.UnCertain.Id;
        var weak = rows
            .GroupBy(r => r.FlashcardId)
            .Select(g => g.MaxBy(r => r.AnsweredAt)!)
            .Where(r => r.AnswerTypeId == wrong || r.AnswerTypeId == guess)
            .ToList();
        if (weak.Count == 0)
            return [];

        var ids = weak.Select(w => w.FlashcardId).ToList();
        var stillLearning = (await Context.LearningSchedules
                .Where(s => ids.Contains(s.FlashcardId) && s.Status == LearningOnly)
                .Select(s => s.FlashcardId)
                .ToListAsync(ct))
            .ToHashSet();

        return weak
            .Where(w => stillLearning.Contains(w.FlashcardId))
            .Select(w => new WeakAnswerReadModel(w.FlashcardId, w.BoxId, w.AnswerTypeId))
            .ToList();
    }

    public async Task<IReadOnlyList<CategoryBoxReadModel>> GetCategoryBoxesAsync(long userId, CancellationToken ct)
    {
        return await Context.CategoryBoxes
            .Where(cb => cb.Category.UserId == userId)
            .Select(cb => new CategoryBoxReadModel(cb.CategoryId, cb.BoxId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyDictionary<long, BoxTodayPlanDto>> GetBoxPlansAsync(bool isCategory, long scopeId, CancellationToken ct)
    {
        var boxes = isCategory
            ? Context.Boxes.Where(b => b.CategoryBoxes.Any(cb => cb.CategoryId == scopeId))
            : Context.Boxes.Where(b => b.Id == scopeId);

        var plans = await NewCardAllowance.ForBoxesAsync(Context, boxes, DateTime.UtcNow, ct);
        return plans.ToDictionary(q => q.Key, q => new BoxTodayPlanDto(q.Value.ToDto(), q.Value.BacklogDto()));
    }

    public async Task<IReadOnlyList<ScopeBoxReadModel>> GetScopeBoxesAsync(bool isCategory, long scopeId, CancellationToken ct)
    {
        var boxes = isCategory
            ? Context.Boxes.Where(b => b.CategoryBoxes.Any(cb => cb.CategoryId == scopeId))
            : Context.Boxes.Where(b => b.Id == scopeId);

        return await boxes
            .OrderBy(b => b.OrderBox)
            .Select(b => new ScopeBoxReadModel(b.Id, b.Name))
            .ToListAsync(ct);
    }

    /// <summary>
    /// کارت‌هایی از این جعبه‌ها که امروز (محلی) جواب گرفته‌اند، هر کدام با گامی که الان در آن است
    /// (یادگرفته‌شده = گام ۹) و تعداد «اشتباه»های امروزش.
    /// </summary>
    public async Task<IReadOnlyList<AnsweredTodayReadModel>> GetAnsweredTodayAsync(IReadOnlyCollection<long> boxIds, DateOnly today, CancellationToken ct)
    {
        var wrongId = AnswerTypeEnum.Wrong.Id;

        var logs = await Context.ReviewLogs
            .Where(r => boxIds.Contains(r.BoxId) && r.ReviewDate == today)
            .Select(r => new { r.FlashcardId, r.BoxId, r.AnswerTypeId, IsReview = r.PreviousAnsweredAt != null })
            .ToListAsync(ct);

        var flashcardIds = logs.Select(l => l.FlashcardId).Distinct().ToList();
        var schedules = await Context.LearningSchedules
            .Where(l => flashcardIds.Contains(l.FlashcardId))
            .Select(l => new { l.FlashcardId, l.NextAnswerDate, l.Interval, l.Status })
            .ToDictionaryAsync(l => l.FlashcardId, ct);

        // کارتی که بعد از جواب حذف شده، دیگر گامی ندارد
        return logs
            .GroupBy(l => l.FlashcardId)
            .Where(g => schedules.ContainsKey(g.Key))
            .Select(g =>
            {
                var s = schedules[g.Key];
                return new AnsweredTodayReadModel(
                    g.Key,
                    g.First().BoxId,
                    LearningStep.For(s.NextAnswerDate == null, s.Interval, (LearningStatus)s.Status),
                    // همان شمارش قاعدهٔ کارت جدید: فقط «اشتباه»ِ مرورها، نه اولین جواب کارت جدید
                    g.Count(l => l.AnswerTypeId == wrongId && l.IsReview));
            })
            .ToList();
    }
}
