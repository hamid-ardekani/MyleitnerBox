using Microsoft.EntityFrameworkCore;
using MyLeitner.Application.Stats.Queries;
using MyLeitner.Application.Stats.Queries.Models;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.StudySessions;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;

namespace MyLeitner.Persistence.ReadModel.Sql.Service;

public class StatsReadService(ReadModelDbContext context) : ReadService(context), IStatsReadService
{
    private const int ForecastDays = 7;

    // یادگرفته‌شده و بایگانی‌شده دیگر موعد مرور ندارند
    private const int LearningOnly = (int)LearningStatus.Learning;

    // «درست» = همه به جز «شانسی» و «اشتباه»
    private static readonly HashSet<short> CorrectAnswerTypeIds =
        [AnswerTypeEnum.Confident.Id, AnswerTypeEnum.NeedMoreReview.Id, AnswerTypeEnum.NotSure.Id];

    public async Task<StatsDto> GetStatsAsync(long userId, StatsPeriod period, DateTime utcNow, CancellationToken cancellationToken)
    {
        var today = StudySessionClock.LocalToday(utcNow);

        var summary = await GetSummaryAsync(userId, today, utcNow, cancellationToken);
        var periodStats = await GetPeriodAsync(userId, period, today, cancellationToken);
        var forecast = await GetForecastAsync(userId, today, cancellationToken);
        var boxes = await GetBoxStepsAsync(userId, cancellationToken);

        return new StatsDto(today, summary, periodStats, forecast, boxes);
    }

    public async Task<ProgressDto> GetProgressAsync(long userId, DateTime utcNow, CancellationToken cancellationToken)
    {
        var today = StudySessionClock.LocalToday(utcNow);

        // فقط تاریخ و نوع پاسخِ کل تاریخچه: رکوردها و میانگین‌ها از نقطه‌های روز (جمع AnswerQuality) می‌آیند
        var answers = await Context.ReviewLogs
            .Where(r => r.UserId == userId && r.ReviewDate <= today)
            .Select(r => new { r.ReviewDate, r.AnswerTypeId })
            .ToListAsync(cancellationToken);
        var perDay = answers.GroupBy(a => a.ReviewDate).ToDictionary(g => g.Key, g => g.Sum(a => AnswerQuality.Score(a.AnswerTypeId)));

        var goal = await Context.ReviewGoals.FirstOrDefaultAsync(g => g.UserId == userId, cancellationToken);
        var goals = goal == null ? ProgressCalculator.DefaultGoals : new ReviewGoalsDto(goal.Daily, goal.Weekly, goal.Monthly);

        return ProgressCalculator.Compute(perDay, today, goals);
    }

    public async Task<HomeDto> GetHomeAsync(long userId, DateTime utcNow, CancellationToken cancellationToken)
    {
        var ct = cancellationToken;
        var today = StudySessionClock.LocalToday(utcNow);
        var progress = await GetProgressAsync(userId, utcNow, ct);

        // هر روزی که جواب داشته: برای زنجیره و مجموع
        var allDays = await Context.ReviewLogs
            .Where(r => r.UserId == userId && r.ReviewDate <= today)
            .GroupBy(r => r.ReviewDate)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var daysDescending = allDays.Select(d => d.Day).OrderByDescending(d => d).ToList();
        var total = allDays.Sum(d => d.Count);

        // جواب‌هایی که کارت را حداقل یک گام جلو بردند این هفته (شنبه تا امروز)
        var weekFrom = StatsPeriodRange.For(StatsPeriod.Week, today).From;
        var weekLogs = await Context.ReviewLogs
            .Where(r => r.UserId == userId && r.ReviewDate >= weekFrom && r.ReviewDate <= today)
            .Select(r => new { r.PreviousAnsweredAt, r.IntervalBefore, r.IntervalAfter })
            .ToListAsync(ct);
        var stepsUpWeek = weekLogs.Count(l =>
            LearningStep.ForInterval(l.IntervalAfter) > (l.PreviousAnsweredAt == null ? LearningStep.New : LearningStep.ForInterval(l.IntervalBefore)));

        var mastered = await Context.LearningSchedules
            .CountAsync(s => s.UserId == userId && s.Status == (int)LearningStatus.Mastered, ct);

        // آنچه امروز در صف مانده، به تفکیک جعبه (مرورها + کارت‌های جدیدی که واقعاً امروز وارد می‌شوند)
        var userBoxes = Context.Boxes.Where(b => b.UserId == userId);
        var allowance = await NewCardAllowance.ForBoxesAsync(Context, userBoxes, utcNow, ct);
        var names = await userBoxes.OrderBy(b => b.OrderBox).Select(b => new { b.Id, b.Name }).ToListAsync(ct);
        var boxes = names
            .Select(b => new HomeBoxDto(b.Id, b.Name, allowance.TryGetValue(b.Id, out var a) ? a.VisibleReviews + a.AdmittedToday : 0))
            .Where(b => b.Remaining > 0)
            .OrderByDescending(b => b.Remaining)
            .ToList();

        // دسته‌هایی که امروز کار دارند: جمع جعبه‌هایشان (جعبهٔ دو دسته در هر دو حساب می‌شود)
        var remainingByBox = boxes.ToDictionary(b => b.BoxId, b => b.Remaining);
        var categoryRows = await Context.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Order)
            .Select(c => new { c.Id, c.Name, BoxIds = c.CategoryBoxes.Select(cb => cb.BoxId).ToList() })
            .ToListAsync(ct);
        var categories = categoryRows
            .Select(c => new HomeCategoryDto(c.Id, c.Name, c.BoxIds.Sum(id => remainingByBox.GetValueOrDefault(id))))
            .Where(c => c.Remaining > 0)
            .OrderByDescending(c => c.Remaining)
            .ToList();

        // «ادامهٔ مرور» به جایی می‌رود که کاربر مرور می‌کرد. مرور دسته جعبه‌هایش را قاطی می‌کند (پرکارترین جعبه وسط مرور عوض می‌شود)
        // و مروری که از دسته شروع شده نباید به خاطر جعبه‌ای که بعدش زده شد گم شود، پس:
        // ۱) دستهٔ آخرین جواب دسته در امروز، اگر هنوز کار دارد؛ ۲) وگرنه جعبه/دستهٔ آخرین جواب هر روز، اگر هنوز کار دارد؛
        // ۳) وگرنه null (وب پرکارترین جعبه را می‌گیرد)
        var lastCategoryToday = await Context.ReviewLogs
            .Where(r => r.UserId == userId && r.ReviewDate == today && r.CategoryId != null)
            .OrderByDescending(r => r.AnsweredAt)
            .Select(r => r.CategoryId)
            .FirstOrDefaultAsync(ct);
        var lastAnswer = await Context.ReviewLogs
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.AnsweredAt)
            .Select(r => new { r.BoxId, r.CategoryId })
            .FirstOrDefaultAsync(ct);

        HomeResumeDto? resume = null;
        if ((lastCategoryToday ?? lastAnswer?.CategoryId) is long categoryId)
        {
            var category = categories.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category != null)
                resume = new HomeResumeDto("category", categoryId, category.Name, category.Remaining);
        }
        if (resume == null && lastAnswer is { CategoryId: null })
        {
            var lastBox = boxes.FirstOrDefault(b => b.BoxId == lastAnswer.BoxId);
            if (lastBox != null)
                resume = new HomeResumeDto("box", lastBox.BoxId, lastBox.Name, lastBox.Remaining);
        }

        return new HomeDto(
            progress,
            CountStreak(daysDescending, today),
            BestStreak(daysDescending),
            total,
            NextMilestone(total),
            mastered,
            stepsUpWeek,
            boxes.Sum(b => b.Remaining),
            boxes,
            categories,
            resume);
    }

    /// <summary>اولین عدد رند بالاتر از مجموع: ۱۰۰، ۵۰۰، بعد هر ۱۰۰۰.</summary>
    private static int NextMilestone(int total) => total < 100 ? 100 : total < 500 ? 500 : (total / 1000 + 1) * 1000;

    private const int RecapWeekDays = 7;
    private const int RecapMonthDays = 30;

    public async Task<RecapDto> GetRecapAsync(long userId, DateTime utcNow, CancellationToken cancellationToken)
    {
        var ct = cancellationToken;
        var today = StudySessionClock.LocalToday(utcNow);
        var monthStart = today.AddDays(-RecapMonthDays);
        var todayStartUtc = StudySessionClock.UtcStartOf(today);

        // جواب‌های ۳۱ روز اخیر، ردیف‌های سبک (حداکثر چند هزار)
        var logs = await Context.ReviewLogs
            .Where(r => r.UserId == userId && r.ReviewDate >= monthStart && r.ReviewDate <= today)
            .Select(r => new { r.ReviewDate, r.AnswerTypeId, r.ResponseTimeInSeconds, r.PreviousAnsweredAt, r.IntervalBefore, r.IntervalAfter, r.DueAt })
            .ToListAsync(ct);

        var perDay = logs.GroupBy(l => l.ReviewDate).ToDictionary(g => g.Key, g => g.Count());
        int CountOn(DateOnly day) => perDay.GetValueOrDefault(day);

        double? Quality(IEnumerable<short> answers)
        {
            var list = answers.ToList();
            return list.Count == 0 ? null : Math.Round(list.Average(a => AnswerQuality.Score(a)) * 100, 1);
        }

        RecapPeriodDto Period(int days)
        {
            var from = today.AddDays(-days);
            var inRange = logs.Where(l => l.ReviewDate >= from && l.ReviewDate < today).ToList();
            var active = inRange.Select(l => l.ReviewDate).Distinct().Count();
            return new RecapPeriodDto(days, active, active == 0 ? null : Math.Round((double)inRange.Count / active, 1), Quality(inRange.Select(l => l.AnswerTypeId)));
        }

        var todayLogs = logs.Where(l => l.ReviewDate == today).ToList();
        var stepsUp = todayLogs.Count(l =>
            LearningStep.ForInterval(l.IntervalAfter) > (l.PreviousAnsweredAt == null ? LearningStep.New : LearningStep.ForInterval(l.IntervalBefore)));
        var mastered = await Context.LearningSchedules
            .CountAsync(s => s.UserId == userId && s.Status == (int)LearningStatus.Mastered && s.StatusChangedAt >= todayStartUtc, ct);
        var todayStats = new RecapTodayDto(todayLogs.Count, todayLogs.Sum(l => l.ResponseTimeInSeconds), Quality(todayLogs.Select(l => l.AnswerTypeId)), stepsUp, mastered);

        var last7 = Enumerable.Range(0, RecapWeekDays)
            .Select(i => today.AddDays(i - (RecapWeekDays - 1)))
            .Select(day => new DayCountDto(day, CountOn(day)))
            .ToList();

        // کارت‌های عقب‌افتاده‌ای که این هفته جواب گرفتند: موعدشان قبل از روز جواب بوده
        var weekFrom = today.AddDays(-(RecapWeekDays - 1));
        var backlogClearedWeek = logs.Count(l =>
            l.ReviewDate >= weekFrom && l.PreviousAnsweredAt != null &&
            (l.DueAt ?? l.PreviousAnsweredAt.Value.AddDays(l.IntervalBefore)) < StudySessionClock.UtcStartOf(l.ReviewDate));

        var isMonthRecord = todayLogs.Count > 0 && perDay.Where(p => p.Key < today).All(p => p.Value < todayLogs.Count);

        // کل تاریخچه: هر روزی که جواب داشته (زنجیره) و مجموع (milestone)
        var allDays = await Context.ReviewLogs
            .Where(r => r.UserId == userId && r.ReviewDate <= today)
            .GroupBy(r => r.ReviewDate)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var daysDescending = allDays.Select(d => d.Day).OrderByDescending(d => d).ToList();
        var total = allDays.Sum(d => d.Count);
        var milestone = Milestone(total - todayLogs.Count, total);

        var newCards = await NewCardAllowance.ForBoxesAsync(Context, Context.Boxes.Where(b => b.UserId == userId), utcNow, ct);
        var tomorrowStart = StudySessionClock.UtcStartOf(today.AddDays(1));
        var dayAfterTomorrowStart = StudySessionClock.UtcStartOf(today.AddDays(2));
        var dueTomorrow = await Context.LearningSchedules
            .CountAsync(l => l.UserId == userId && l.Status == LearningOnly && l.NextAnswerDate >= tomorrowStart && l.NextAnswerDate < dayAfterTomorrowStart, ct);

        return new RecapDto(
            today,
            todayStats,
            CountOn(today.AddDays(-1)),
            last7,
            Period(RecapWeekDays),
            Period(RecapMonthDays),
            CountStreak(daysDescending, today),
            BestStreak(daysDescending),
            backlogClearedWeek,
            newCards.Values.Sum(b => b.Backlog.Remaining),
            isMonthRecord,
            milestone,
            dueTomorrow);
    }

    /// <summary>عدد رندی که امروز از آن گذشته شد (اگر باشد): ۱۰۰، ۵۰۰، ۱۰۰۰، بعد هر ۱۰۰۰.</summary>
    private static int? Milestone(int before, int after)
    {
        int? crossed = null;
        foreach (var m in new[] { 100, 500 }.Concat(Enumerable.Range(1, Math.Max(0, after / 1000)).Select(k => k * 1000)))
            if (before < m && after >= m)
                crossed = m;
        return crossed;
    }

    /// <summary>طولانی‌ترین زنجیرهٔ روزهای پشت‌سرهم با جواب، در کل تاریخچه.</summary>
    private static int BestStreak(List<DateOnly> daysDescending)
    {
        var best = 0;
        var run = 0;
        DateOnly? previous = null;
        foreach (var day in daysDescending)
        {
            run = previous is { } p && p.AddDays(-1) == day ? run + 1 : 1;
            best = Math.Max(best, run);
            previous = day;
        }
        return best;
    }

    private async Task<StatsSummaryDto> GetSummaryAsync(long userId, DateOnly today, DateTime utcNow, CancellationToken ct)
    {
        var tomorrowStart = StudySessionClock.UtcStartOf(today.AddDays(1));
        var dayAfterTomorrowStart = StudySessionClock.UtcStartOf(today.AddDays(2));

        var reviewedToday = await Context.ReviewLogs
            .CountAsync(r => r.UserId == userId && r.ReviewDate == today, ct);


        // کارت‌های جدید فقط به اندازهٔ سهم امروز هر جعبه - همان عددی که مرور واقعاً نشان می‌دهد
        var newCards = await NewCardAllowance.ForBoxesAsync(Context, Context.Boxes.Where(b => b.UserId == userId), utcNow, ct);
        var newCardsToday = newCards.Values.Sum(b => b.AdmittedToday);

        // مرورهایی که واقعاً امروز در صف‌اند: همهٔ سرِ موعدها + فقط سهم آزادشدهٔ بدهی قدیمی
        var dueReviewsToday = newCards.Values.Sum(b => b.VisibleReviews);
        var backlogRemaining = newCards.Values.Sum(b => b.Backlog.Remaining);

        var dueTomorrow = await Context.LearningSchedules
            .CountAsync(l => l.UserId == userId && l.Status == LearningOnly && l.NextAnswerDate >= tomorrowStart && l.NextAnswerDate < dayAfterTomorrowStart, ct);

        var reviewDays = await Context.ReviewLogs
            .Where(r => r.UserId == userId && r.ReviewDate <= today)
            .Select(r => r.ReviewDate)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync(ct);

        return new StatsSummaryDto(reviewedToday, dueReviewsToday, newCardsToday, CountStreak(reviewDays, today), dueTomorrow, backlogRemaining);
    }

    /// <summary>روزهای پشت‌سرهم تا امروز؛ اگر امروز هنوز مرور نشده، از دیروز حساب می‌شود (هنوز نشکسته).</summary>
    private static int CountStreak(List<DateOnly> reviewDaysDescending, DateOnly today)
    {
        if (reviewDaysDescending.Count == 0)
            return 0;

        var expected = reviewDaysDescending[0] == today ? today : today.AddDays(-1);
        var streak = 0;

        foreach (var day in reviewDaysDescending)
        {
            if (day != expected)
                break;

            streak++;
            expected = expected.AddDays(-1);
        }

        return streak;
    }

    private async Task<StatsPeriodDto> GetPeriodAsync(long userId, StatsPeriod period, DateOnly today, CancellationToken ct)
    {
        var (from, to) = StatsPeriodRange.For(period, today);

        // برنامهٔ شخصی است و حجم یک سال پاسخ کم است؛ ردیف‌های سبک را می‌خوانیم و در حافظه جمع می‌زنیم
        var logs = await Context.ReviewLogs
            .Where(r => r.UserId == userId && r.ReviewDate >= from && r.ReviewDate <= to)
            .Select(r => new { r.ReviewDate, r.AnswerTypeId, r.FlashcardId, r.ResponseTimeInSeconds })
            .ToListAsync(ct);

        var fromUtc = StudySessionClock.UtcStartOf(from);
        var toUtcExclusive = StudySessionClock.UtcStartOf(to.AddDays(1));
        var cardsAdded = await Context.Flashcards
            .CountAsync(f => f.Box.UserId == userId && f.CreatedAt >= fromUtc && f.CreatedAt < toUtcExclusive, ct);

        var answerTypes = logs
            .GroupBy(l => l.AnswerTypeId)
            .Select(g => new AnswerTypeCountDto(g.Key, g.Count()))
            .OrderBy(a => a.AnswerTypeId)
            .ToList();

        double? correctPercent = logs.Count == 0
            ? null
            : Math.Round(100.0 * logs.Count(l => CorrectAnswerTypeIds.Contains(l.AnswerTypeId)) / logs.Count, 1);

        double? averageResponse = logs.Count == 0
            ? null
            : Math.Round(logs.Average(l => l.ResponseTimeInSeconds), 1);

        var buckets = StatsPeriodRange.Buckets(period, from, to)
            .Select(b => new StatsBucketDto(b.From, b.To, logs.Count(l => l.ReviewDate >= b.From && l.ReviewDate <= b.To)))
            .ToList();

        return new StatsPeriodDto(
            from,
            to,
            logs.Count,
            logs.Select(l => l.FlashcardId).Distinct().Count(),
            answerTypes,
            correctPercent,
            averageResponse,
            logs.Select(l => l.ReviewDate).Distinct().Count(),
            cardsAdded,
            buckets,
            period == StatsPeriod.Year ? DailyCounts(from, to, logs.Select(l => l.ReviewDate)) : []);
    }

    private static List<DayCountDto> DailyCounts(DateOnly from, DateOnly to, IEnumerable<DateOnly> reviewDates)
    {
        var perDay = reviewDates.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());
        var days = new List<DayCountDto>();
        for (var day = from; day <= to; day = day.AddDays(1))
            days.Add(new DayCountDto(day, perDay.GetValueOrDefault(day)));
        return days;
    }

    /// <summary>
    /// هر جعبه: کارت‌های در حال یادگیری به تفکیک گام، به‌اضافهٔ یادگرفته‌شده و بایگانی.
    /// </summary>
    private async Task<List<BoxStepsDto>> GetBoxStepsAsync(long userId, CancellationToken ct)
    {
        var boxes = await Context.Boxes
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.OrderBox)
            .Select(b => new { b.Id, b.Name })
            .ToListAsync(ct);

        var cards = await Context.Flashcards
            .Where(f => f.Box.UserId == userId && f.LearningSchedule != null)
            .Select(f => new
            {
                f.BoxId,
                NeverAnswered = f.LearningSchedule!.NextAnswerDate == null,
                f.LearningSchedule.Interval,
                f.LearningSchedule.Status,
            })
            .ToListAsync(ct);

        var quotas = await NewCardAllowance.ForBoxesAsync(Context, Context.Boxes.Where(b => b.UserId == userId), DateTime.UtcNow, ct);

        return boxes
            .Select(b =>
            {
                var inBox = cards.Where(c => c.BoxId == b.Id).ToList();
                var steps = new int[LearningStep.Last + 1];
                foreach (var c in inBox.Where(c => c.Status == LearningOnly))
                    steps[LearningStep.For(c.NeverAnswered, c.Interval)]++;

                return new BoxStepsDto(
                    b.Id,
                    b.Name,
                    steps.ToList(),
                    inBox.Count(c => c.Status == (int)LearningStatus.Mastered),
                    inBox.Count(c => c.Status == (int)LearningStatus.Archived),
                    quotas.TryGetValue(b.Id, out var quota) ? quota.ToDto() : null,
                    quota?.BacklogDto());
            })
            .ToList();
    }

    /// <summary>
    /// تعداد کارت‌هایی که در هر یک از ۷ روز آینده (از فردا) موعدشان می‌رسد.
    /// </summary>
    private async Task<List<DayCountDto>> GetForecastAsync(long userId, DateOnly today, CancellationToken ct)
    {
        var firstDay = today.AddDays(1);
        var fromUtc = StudySessionClock.UtcStartOf(firstDay);
        var toUtcExclusive = StudySessionClock.UtcStartOf(firstDay.AddDays(ForecastDays));

        var dueDates = await Context.LearningSchedules
            .Where(l => l.UserId == userId && l.Status == LearningOnly && l.NextAnswerDate >= fromUtc && l.NextAnswerDate < toUtcExclusive)
            .Select(l => l.NextAnswerDate!.Value)
            .ToListAsync(ct);

        var perDay = dueDates
            .GroupBy(StudySessionClock.LocalToday)
            .ToDictionary(g => g.Key, g => g.Count());

        return Enumerable.Range(0, ForecastDays)
            .Select(i => firstDay.AddDays(i))
            .Select(day => new DayCountDto(day, perDay.GetValueOrDefault(day)))
            .ToList();
    }
}
