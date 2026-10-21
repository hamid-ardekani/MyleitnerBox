using Microsoft.EntityFrameworkCore;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.StudySessions;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;
using MyLeitner.Persistence.ReadModel.Sql.ReadModels;
using DomainBox = MyLeitner.Domain.Boxes.Box;
using NewCardQuota = MyLeitner.Domain.Boxes.NewCardQuota;
using BacklogQuota = MyLeitner.Domain.Boxes.BacklogQuota;
using MyLeitner.Application.Contract.Dtos;

namespace MyLeitner.Persistence.ReadModel.Sql.Service;

/// <param name="Quota">سهم کارت جدید امروز و دلیل‌هایش (<see cref="NewCardQuota"/>).</param>
/// <param name="Waiting">چند کارت جدید (هیچ‌وقت پاسخ‌نگرفته) در این جعبه هست.</param>
/// <param name="Backlog">سهم امروزِ کارت‌های عقب‌افتاده (<see cref="BacklogQuota"/>).</param>
/// <param name="DueTodayPending">مرورهای سرِ موعدِ امروز که هنوز مانده‌اند.</param>
internal record BoxNewCards(long BoxId, NewCardQuota Quota, int Waiting, BacklogQuota Backlog, int DueTodayPending)
{
    /// <summary>چند کارت جدید دیگر امروز از این جعبه مجاز است.</summary>
    public int Allowed => Quota.Allowed;

    /// <summary>مرورهایی که امروز واقعاً در صف‌اند: سرِ موعدها + سهم آزادشدهٔ بدهی.</summary>
    public int VisibleReviews => DueTodayPending + Backlog.Available;

    public NewCardQuotaDto ToDto() => new(
        Quota.BaseLimit, Quota.Target, Quota.Allowed, Waiting, Quota.DueReviews,
        Quota.WorkloadFactor, Quota.AccuracyFactor, Quota.RecentAccuracyPercent, Quota.StoppedByWrongs);

    public BacklogQuotaDto BacklogDto() => new(
        Backlog.Floor, Backlog.Released, Backlog.DoneToday, Backlog.Remaining, Backlog.Available,
        Backlog.Pace.ToString(), Backlog.RecentAccuracyPercent, Backlog.MoreAvailable);

    /// <summary>کارت‌های جدیدی که امروز واقعاً وارد مرور می‌شوند.</summary>
    public int AdmittedToday => Math.Min(Allowed, Waiting);
}

/// <summary>
/// برنامهٔ امروزِ هر جعبه - سهم بدهی و سهم کارت جدید. هم انتخاب «کارت بعدی» و هم نمای کلی/داشبورد از همین
/// استفاده می‌کنند تا همه یک عدد ببینند.
/// </summary>
internal static class NewCardAllowance
{
    public static async Task<Dictionary<long, BoxNewCards>> ForBoxesAsync(
        ReadModelDbContext context, IQueryable<Box> boxes, DateTime utcNow, CancellationToken ct)
    {
        var today = StudySessionClock.LocalToday(utcNow);
        var todayStart = StudySessionClock.UtcStartOf(today);
        var tomorrowStart = StudySessionClock.UtcStartOf(today.AddDays(1));
        var wrongId = AnswerTypeEnum.Wrong.Id;
        const int learning = (int)LearningStatus.Learning;

        var perBox = await boxes
            .Select(b => new
            {
                b.Id,
                b.DailyNewCardLimit,
                b.DailyBacklogFloor,
                b.BacklogBonusDate,
                b.BacklogBonus,
                DueTodayPending = b.Flashcards.Count(f =>
                    f.LearningSchedule != null && f.LearningSchedule.Status == learning
                    && f.LearningSchedule.NextAnswerDate >= todayStart && f.LearningSchedule.NextAnswerDate < tomorrowStart),
                Backlog = b.Flashcards.Count(f =>
                    f.LearningSchedule != null && f.LearningSchedule.Status == learning
                    && f.LearningSchedule.NextAnswerDate != null && f.LearningSchedule.NextAnswerDate < todayStart),
                Waiting = b.Flashcards.Count(f =>
                    f.LearningSchedule != null && f.LearningSchedule.Status == learning && f.LearningSchedule.NextAnswerDate == null),
            })
            .ToListAsync(ct);

        var boxIds = perBox.Select(b => b.Id).ToList();
        var confident = AnswerTypeEnum.Confident.Id;
        var needMoreReview = AnswerTypeEnum.NeedMoreReview.Id;
        var notSure = AnswerTypeEnum.NotSure.Id;

        // دقت برای سهم کارت جدید: فقط مرورهای ۷ روز اخیر، نه اولین جواب کارت جدید
        var windowStart = today.AddDays(1 - NewCardQuota.AccuracyWindowDays);
        var recentByBox = await context.ReviewLogs
            .Where(r => boxIds.Contains(r.BoxId) && r.ReviewDate >= windowStart && r.ReviewDate <= today && r.PreviousAnsweredAt != null)
            .GroupBy(r => r.BoxId)
            .Select(g => new
            {
                BoxId = g.Key,
                Reviews = g.Count(),
                Correct = g.Count(r => r.AnswerTypeId == confident || r.AnswerTypeId == needMoreReview || r.AnswerTypeId == notSure),
            })
            .ToDictionaryAsync(x => x.BoxId, ct);

        // جواب‌های امروز (کم‌حجم): کارت‌های جدید شروع‌شده، مرورهای اشتباه، بدهی صاف‌شده و دقت آخرین مرورها
        var todayLogs = await context.ReviewLogs
            .Where(r => boxIds.Contains(r.BoxId) && r.ReviewDate == today)
            .Select(r => new { r.BoxId, r.AnsweredAt, r.AnswerTypeId, r.PreviousAnsweredAt, r.IntervalBefore, r.DueAt })
            .ToListAsync(ct);

        return perBox.ToDictionary(
            b => b.Id,
            b =>
            {
                var logs = todayLogs.Where(l => l.BoxId == b.Id).ToList();
                var reviewsNewestFirst = logs.Where(l => l.PreviousAnsweredAt != null).OrderByDescending(l => l.AnsweredAt).ToList();

                // مرور وقتی بدهی را صاف کرده که موعد کارت قبل از امروز بوده (لاگ‌های قدیمی DueAt ندارند: از جواب قبلی + فاصله‌ای که داشت تخمین زده می‌شود)
                var backlogDone = reviewsNewestFirst.Count(l =>
                    (l.DueAt ?? l.PreviousAnsweredAt!.Value.AddDays(l.IntervalBefore)) < todayStart);

                var backlog = BacklogQuota.Compute(
                    b.DailyBacklogFloor,
                    b.Backlog,
                    backlogDone,
                    reviewsNewestFirst.Select(l => AnswerQuality.Score(l.AnswerTypeId)).ToList(),
                    DomainBox.BacklogBonusFor(b.BacklogBonusDate, b.BacklogBonus, today));

                // بار کارِ سهم کارت جدید فقط مرورهایی را می‌شمرد که واقعاً امروز در صف‌اند (سرِ موعد + بدهی آزادشده)،
                // نه کل بدهی - وگرنه جعبه‌ای با بدهی قدیمی تا صاف شدن همه‌اش کارت جدید نمی‌گرفت
                var visibleReviews = b.DueTodayPending + backlog.Available;
                var recent = recentByBox.GetValueOrDefault(b.Id);
                var quota = DomainBox.NewCardQuotaToday(
                    b.DailyNewCardLimit,
                    visibleReviews,
                    reviewsNewestFirst.Count(l => l.AnswerTypeId == wrongId),
                    logs.Count(l => l.PreviousAnsweredAt == null),
                    recent?.Reviews ?? 0,
                    recent?.Correct ?? 0);

                return new BoxNewCards(b.Id, quota, b.Waiting, backlog, b.DueTodayPending);
            });
    }
}
