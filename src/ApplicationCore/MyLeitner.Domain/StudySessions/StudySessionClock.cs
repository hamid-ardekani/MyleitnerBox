namespace MyLeitner.Domain.StudySessions;

/// <summary>«امروز محلی» برای کلید <see cref="StudySession.SessionDate"/>.</summary>
/// <remarks>
/// فعلاً منطقهٔ زمانی همهٔ کاربران تهران (+۰۳:۳۰) فرض می‌شود چون timezone واقعی ذخیره نمی‌شود؛ با اضافه‌شدن آن فقط همین کلاس عوض می‌شود.
/// </remarks>
public static class StudySessionClock
{
    private static readonly TimeSpan TehranOffset = new(3, 30, 0);

    public static DateOnly LocalToday(DateTime utcNow) => DateOnly.FromDateTime(utcNow + TehranOffset);

    /// <summary>لحظهٔ شروع (نیمه‌شب محلی) یک روز محلی به UTC، برای مقایسه با ستون‌های DateTime ذخیره‌شده به UTC.</summary>
    public static DateTime UtcStartOf(DateOnly localDate) => localDate.ToDateTime(TimeOnly.MinValue) - TehranOffset;
}
