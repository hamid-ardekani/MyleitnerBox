namespace MyLeitner.Domain.Boxes;

public enum BacklogPace
{
    /// <summary>هنوز ۱۰ مرور امروز جمع نشده - فقط کف آزاد است.</summary>
    Warmup = 0,

    /// <summary>دقت ۱۰ مرور آخر ۸۰٪+ - ۱۰ کارت جلوتر باز می‌شود.</summary>
    Growing = 1,

    /// <summary>دقت ۶۰ تا ۸۰٪ - ۵ کارت جلوتر.</summary>
    Steady = 2,

    /// <summary>دقت زیر ۶۰٪ - برای امروز بیشتر باز نمی‌شود («برای امروز کافیه»، نه تنبیه).</summary>
    Resting = 3,

    /// <summary>به سقف روزانه رسید.</summary>
    Capped = 4
}

/// <summary>
/// سهم امروزِ «بدهی» یک جعبه (کارت‌هایی که موعدشان <b>قبل از امروز</b> بوده). کارت‌های سرِ موعدِ امروز همیشه کامل نشان داده می‌شوند؛
/// فقط بدهی تکه‌تکه آزاد می‌شود تا «امروز ۳۳۷ کارت» کاربر را دلزده نکند و کف ثابت هم دچار قحطی نشود.
/// <list type="bullet">
/// <item>آزادشده = <see cref="Floor"/> + «هدیه‌ای» که کاربر امروز در پایان مرور قبول کرده؛ حداکثر <see cref="DailyCap"/>.
/// دقت ۱۰ مرور آخر فقط تعیین می‌کند آخرِ مرور چند کارت پیشنهاد شود: ۸۰٪+ (یا هنوز ۱۰ مرور نشده) → ۱۰، ۶۰–۸۰٪ → ۵، زیر ۶۰٪ → هیچ.</item>
/// <item>دقت از همهٔ مرورهای امروز آن جعبه حساب می‌شود (نه فقط بدهی) و وزن‌دار است
/// (<see cref="Flashcards.AnswerQuality"/>: مطمئن ۱، مرور ۰٫۷۵، شک ۰٫۵، شانسی/اشتباه ۰).</item>
/// </list>
/// </summary>
/// <param name="Floor">کفِ امروز (از تنظیم جعبه).</param>
/// <param name="Released">چند کارت بدهی امروز در کل آزاد است (شامل صاف‌شده‌ها).</param>
/// <param name="DoneToday">چند کارت بدهی امروز جواب گرفته.</param>
/// <param name="Remaining">کارت‌های بدهی که هنوز مانده‌اند (کل، نه فقط آزادشده‌ها).</param>
/// <param name="RecentAccuracyPercent">دقت ۱۰ مرور آخر امروز؛ null در گرم‌کردن.</param>
public sealed record BacklogQuota(int Floor, int Released, int DoneToday, int Remaining, BacklogPace Pace, double? RecentAccuracyPercent)
{
    public const short DefaultFloor = 20;
    public const short MinFloor = 1;
    public const short MaxFloor = 99;

    public const int DailyCap = 80;
    public const int RecentWindow = 10;
    private const double GoodAccuracy = 0.8;
    private const double OkAccuracy = 0.6;
    private const int GoodLookahead = 10;
    private const int OkLookahead = 5;

    /// <summary>چند کارت بدهیِ دیگر همین حالا باید در صف باشد.</summary>
    public int Available => Math.Min(Remaining, Math.Max(0, Released - DoneToday));

    /// <summary>
    /// چند کارت عقب‌افتادهٔ دیگر را می‌شود در پایان مرور پیشنهاد داد؛ ۰ = پیشنهادی نیست
    /// (روز بد <see cref="BacklogPace.Resting"/>، سقف روزانه، یا چیزی نمانده).
    /// </summary>
    public int MoreAvailable
    {
        get
        {
            var chunk = Pace switch
            {
                BacklogPace.Warmup or BacklogPace.Growing => GoodLookahead,
                BacklogPace.Steady => OkLookahead,
                _ => 0,
            };
            return Math.Max(0, Math.Min(chunk, Math.Min(DailyCap - Released, Remaining - Available)));
        }
    }

    /// <param name="recentReviewScoresNewestFirst">کیفیت (۰..۱) مرورهای امروز این جعبه، جدیدترین اول (حداکثر ۱۰ تای اول دیده می‌شود).</param>
    /// <param name="bonus">کارت‌هایی که کاربر امروز با «بیشتر می‌خوانم» اضافه قبول کرده.</param>
    public static BacklogQuota Compute(short floor, int remaining, int doneToday, IReadOnlyList<double> recentReviewScoresNewestFirst, int bonus = 0)
    {
        var effectiveFloor = Math.Min((int)floor, DailyCap);
        var released = Math.Min(DailyCap, effectiveFloor + Math.Max(0, bonus));

        if (recentReviewScoresNewestFirst.Count < RecentWindow)
            return new BacklogQuota(effectiveFloor, released, doneToday, remaining, released >= DailyCap ? BacklogPace.Capped : BacklogPace.Warmup, null);

        var accuracy = recentReviewScoresNewestFirst.Take(RecentWindow).Average();
        var pace = accuracy >= GoodAccuracy ? BacklogPace.Growing
            : accuracy >= OkAccuracy ? BacklogPace.Steady
            : BacklogPace.Resting;
        if (released >= DailyCap && pace != BacklogPace.Resting) pace = BacklogPace.Capped;

        return new BacklogQuota(effectiveFloor, released, doneToday, remaining, pace, Math.Round(accuracy * 100));
    }
}
