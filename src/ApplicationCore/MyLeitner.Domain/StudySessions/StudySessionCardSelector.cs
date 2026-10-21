using MyLeitner.Domain.Flashcards;

namespace MyLeitner.Domain.StudySessions;

/// <summary>انتخاب «فلش‌کارت بعدی» از کاندیدهای موعدرسیدهٔ یک <see cref="StudySession"/>.</summary>
/// <remarks>
/// قاعده‌ها به ترتیب:
/// ۱) کارت‌هایی که امروز دیده نشده‌اند بر کارت‌های Deferred مقدم‌اند. وقتی فقط Deferredها مانده‌اند، آن که زودتر «بعداً» خورده اول می‌آید
///    (نه همان کارتی که همین الان رد شد). کارت‌های Answered امروز کاندیدا نیستند و برای اطمینان دوباره فیلتر می‌شوند.
/// ۲) کارتی که پشت‌سرهم چند بار از یک باکس آمده پنالتی می‌گیرد تا باکس‌های دیگر هم فرصت پیدا کنند.
/// ۳) اولویت لحظه‌ای = سختی + معکوس پایداری + هرچه از موعدش گذشته باشد.
/// ۴) کارت جدید: تا مرور مانده فقط به‌عنوان <b>جایزهٔ کسب‌شده</b> می‌آید: بعد از ۵ مرورِ تازه از آخرین کارت جدید، اگر میانگین کیفیتشان
///    (<see cref="AnswerQuality"/>) حداقل ۰٫۷ و هیچ‌کدام «اشتباه» نبوده. وقتی مرورها تمام شد، بقیهٔ سهم کارت جدید امروز می‌آید.
/// </remarks>
public static class StudySessionCardSelector
{
    private const int MaxConsecutivePerBox = 3;
    private const double ConsecutivePenaltyWeight = 0.3;
    private const double ConsecutiveHardCap = 100;
    private const double DaysSinceDueWeight = 0.2;

    public const int RewardWindow = 5;
    public const double RewardMinScore = 0.7;

    /// <summary>آیا الان یک کارت جدید «کسب شده»: حداقل ۵ مرور از آخرین کارت جدید، با میانگین کیفیت ۰٫۷+ در ۵ تای آخر و بدون «اشتباه».</summary>
    public static bool NewCardEarned(StudySession session)
    {
        var resolved = session.Cards.OrderBy(c => c.ResolvedAt).ToList();
        var lastNew = resolved.FindLastIndex(c => c.WasNew);
        var reviewsSince = resolved
            .Skip(lastNew + 1)
            .Where(c => !c.WasNew && c.Status == StudySessionCardStatus.Answered && c.AnswerTypeId.HasValue)
            .ToList();

        if (reviewsSince.Count < RewardWindow)
            return false;

        var window = reviewsSince.TakeLast(RewardWindow).ToList();
        if (window.Any(c => c.AnswerTypeId == AnswerTypeEnum.Wrong.Id))
            return false;

        return window.Average(c => AnswerQuality.Score(c.AnswerTypeId!.Value)) >= RewardMinScore;
    }

    public static CardCandidate? SelectNext(StudySession session, IEnumerable<CardCandidate> candidates, DateTime now)
    {
        var answeredTodayIds = session.Cards
            .Where(c => c.Status == StudySessionCardStatus.Answered)
            .Select(c => c.FlashcardId)
            .ToHashSet();

        // زمان آخرین «بعداً» هر کارت امروز
        var deferredAt = session.Cards
            .Where(c => c.Status == StudySessionCardStatus.Deferred)
            .GroupBy(c => c.FlashcardId)
            .ToDictionary(g => g.Key, g => g.Max(c => c.ResolvedAt));

        var lastBoxStreak = GetLastBoxStreak(session);

        var open = candidates.Where(c => !answeredTodayIds.Contains(c.CardId)).ToList();

        // اول کارت‌هایی که «بعداً» نخورده‌اند: مرورها اول؛ کارت جدید تا مرور مانده فقط جایزه است و بعد از تمام شدن مرورها بقیه می‌آید.
        // مرورهای «بعداً»خورده «مانده» حساب نمی‌شوند، وگرنه کارت‌های جدید را می‌بستند و «بعداً» همان مرور را برمی‌گرداند.
        var fresh = open.Where(c => !deferredAt.ContainsKey(c.CardId)).ToList();
        if (fresh.Count > 0)
        {
            // کارت جدیدی که کاربر دستی به مرور امروز اضافه کرده مثل مرور حساب می‌شود: بدون شرط جایزه
            var reviews = fresh.Where(c => !c.IsNew || c.AddedToReviewToday).ToList();
            var newCards = fresh.Where(c => c.IsNew && !c.AddedToReviewToday).ToList();
            var pool = reviews.Count == 0 ? newCards
                : newCards.Count > 0 && NewCardEarned(session) ? newCards
                : reviews;

            return pool
                .OrderByDescending(c => EffectivePriority(c, lastBoxStreak, now))
                .First();
        }

        // فقط کارت‌های «بعداً»خورده مانده‌اند: آن که زودتر رد شده اول، تا «بعداً» همان کارت را فوراً برنگرداند
        return open
            .OrderBy(c => deferredAt[c.CardId])
            .ThenByDescending(c => EffectivePriority(c, lastBoxStreak, now))
            .FirstOrDefault();
    }

    private static double EffectivePriority(CardCandidate candidate, (long? BoxId, int Streak) lastBoxStreak, DateTime now)
    {
        var basePriority = candidate.Difficulty * 0.6 + (candidate.Stability > 0 ? 1.0 / candidate.Stability : 1.0) * 0.4;
        var daysSinceDue = (now - candidate.NextAnswerDate).TotalDays;
        var penalty = GetConsecutiveBoxPenalty(candidate.BoxId, lastBoxStreak);

        return basePriority + daysSinceDue * DaysSinceDueWeight - penalty;
    }

    private static double GetConsecutiveBoxPenalty(long boxId, (long? BoxId, int Streak) lastBoxStreak)
    {
        if (lastBoxStreak.BoxId != boxId)
            return 0;

        return lastBoxStreak.Streak >= MaxConsecutivePerBox ? ConsecutiveHardCap : lastBoxStreak.Streak * ConsecutivePenaltyWeight;
    }

    /// <summary>آخرین باکسِ نمایش‌داده‌شده در این session و تعداد تکرار پشت‌سرهمش.</summary>
    private static (long? BoxId, int Streak) GetLastBoxStreak(StudySession session)
    {
        var recent = session.Cards.OrderByDescending(c => c.ResolvedAt).ToList();

        if (recent.Count == 0)
            return (null, 0);

        var lastBoxId = recent[0].BoxId;
        var streak = recent.TakeWhile(c => c.BoxId == lastBoxId).Count();

        return (lastBoxId, streak);
    }
}
