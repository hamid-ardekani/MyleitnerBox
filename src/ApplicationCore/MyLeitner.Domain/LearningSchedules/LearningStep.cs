namespace MyLeitner.Domain.LearningSchedules;

/// <summary>
/// «گام» لایتنری یک کارت - ذخیره نمی‌شود، از فاصلهٔ فعلی مرور (<see cref="LearningSchedule.Interval"/>) حساب می‌شود
/// تا با لایتنر قدیم (خانه‌های دوبرابری) قابل مقایسه باشد:
/// جدید | ۱: کمتر از ۲ روز | ۲: ۲–۳ | ۳: ۴–۷ | ۴: ۸–۱۵ | ۵: ۱۶–۳۱ | ۶: ۳۲–۶۳ | ۷: ۶۴–۱۲۷ | ۸: ۱۲۸–۱۸۰ | یادگرفته‌شده.
/// </summary>
/// <remarks>کلاینت همین مرزها را برای برچسب «هر ۸ تا ۱۵ روز» تکرار کرده (web/src/utils/learningStep.ts).</remarks>
public static class LearningStep
{
    /// <summary>هنوز هیچ‌وقت پاسخ نگرفته.</summary>
    public const int New = 0;

    public const int Last = 8;

    /// <summary>فقط برای شمارش/نمایش: کارتی که از مرور خارج شد (<see cref="LearningStatus.Mastered"/>).</summary>
    public const int Mastered = 9;

    // مرز بالای (انحصاری) گام‌های ۱ تا ۷ بر حسب روز؛ هرچه بیشتر، گام ۸
    private static readonly double[] UpperBoundsDays = [2, 4, 8, 16, 32, 64, 128];

    public static int ForInterval(double intervalDays)
    {
        for (var i = 0; i < UpperBoundsDays.Length; i++)
            if (intervalDays < UpperBoundsDays[i])
                return i + 1;

        return Last;
    }

    /// <param name="neverAnswered">NextAnswerDate = null یعنی کارت هنوز جدید است.</param>
    public static int For(bool neverAnswered, double intervalDays, LearningStatus status = LearningStatus.Learning)
    {
        if (status == LearningStatus.Mastered)
            return Mastered;

        return neverAnswered ? New : ForInterval(intervalDays);
    }
}
