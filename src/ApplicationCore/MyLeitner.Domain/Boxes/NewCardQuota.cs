namespace MyLeitner.Domain.Boxes;

/// <summary>
/// سهم کارت جدیدِ امروزِ یک جعبه و دلیل‌هایش:
/// <c>هدف = گرد(سقف جعبه × ضریب بار مرور × ضریب دقت)</c>، حداکثر ۹۹؛ مجاز = هدف − کارت‌های جدیدی که امروز شروع شده‌اند.
/// <list type="bullet">
/// <item>ضریب بار مرور (مرورهای موعدرسیدهٔ امروز): تا ۱۰ → ۱، از ۱۰ تا ۲۰ خطی تا ۰، بالای ۲۰ → ۰.</item>
/// <item>ضریب دقت (فقط <b>مرورها</b>ی ۷ روز اخیر، نه اولین جواب کارت‌های جدید): ۹۰٪+ → ۱٫۲، زیر ۷۰٪ → ۰٫۷، بین‌شان → ۱؛
/// با کمتر از ۲۰ مرور → ۱ (داده کافی نیست).</item>
/// <item>بیش از ۵ «اشتباه» امروز → صفر (فقط اشتباهِ مرورها شمرده می‌شود، نه بلد نبودن کارت جدید).</item>
/// </list>
/// </summary>
/// <param name="BaseLimit">سقف روزانه‌ای که کاربر برای جعبه گذاشته.</param>
/// <param name="Target">سهم امروز بعد از ضریب‌ها (پیش از کم کردن کارت‌های شروع‌شده).</param>
/// <param name="Allowed">چند کارت جدید دیگر امروز مجاز است.</param>
/// <param name="RecentAccuracyPercent">درصد درستیِ مرورهای ۷ روز اخیر؛ null وقتی مرور کافی نیست.</param>
/// <param name="StoppedByWrongs">امروز بیش از حد «اشتباه» شده و کارت جدید قطع است.</param>
public sealed record NewCardQuota(
    int BaseLimit,
    int Target,
    int Allowed,
    int DueReviews,
    double WorkloadFactor,
    double AccuracyFactor,
    double? RecentAccuracyPercent,
    bool StoppedByWrongs)
{
    public const int FullWorkloadMaxReviews = 10;
    public const int NoNewCardsFromReviews = 20;
    public const int MaxTodayWrong = 5;

    public const int AccuracyWindowDays = 7;
    public const int MinReviewsForAccuracy = 20;
    private const double HighAccuracy = 0.9;
    private const double LowAccuracy = 0.7;
    private const double HighAccuracyFactor = 1.2;
    private const double LowAccuracyFactor = 0.7;

    private const int MaxTarget = 99;

    /// <param name="recentReviews">مرورهای (نه اولین جواب‌های) ۷ روز اخیر این جعبه.</param>
    /// <param name="recentCorrectReviews">از همان‌ها، چندتا «درست» (مطمئن/مرور/شک).</param>
    public static NewCardQuota Compute(
        short dailyNewCardLimit, int dueReviews, int todayWrong, int newCardsStartedToday, int recentReviews, int recentCorrectReviews)
    {
        var workload = dueReviews <= FullWorkloadMaxReviews ? 1.0
            : dueReviews >= NoNewCardsFromReviews ? 0.0
            : (double)(NoNewCardsFromReviews - dueReviews) / (NoNewCardsFromReviews - FullWorkloadMaxReviews);

        double? accuracy = recentReviews >= MinReviewsForAccuracy ? (double)recentCorrectReviews / recentReviews : null;
        var accuracyFactor = accuracy switch
        {
            null => 1.0,
            >= HighAccuracy => HighAccuracyFactor,
            < LowAccuracy => LowAccuracyFactor,
            _ => 1.0,
        };

        var stoppedByWrongs = todayWrong > MaxTodayWrong;
        var target = Math.Min(MaxTarget, (int)Math.Round(dailyNewCardLimit * workload * accuracyFactor, MidpointRounding.AwayFromZero));
        var allowed = stoppedByWrongs ? 0 : Math.Max(0, target - newCardsStartedToday);

        return new NewCardQuota(
            dailyNewCardLimit,
            target,
            allowed,
            dueReviews,
            workload,
            accuracyFactor,
            accuracy.HasValue ? Math.Round(accuracy.Value * 100, 1) : null,
            stoppedByWrongs);
    }
}
