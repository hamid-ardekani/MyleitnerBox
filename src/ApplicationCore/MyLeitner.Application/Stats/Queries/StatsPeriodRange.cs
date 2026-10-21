using System.Globalization;

namespace MyLeitner.Application.Stats.Queries;

public enum StatsPeriod
{
    Today = 0,
    Week = 1,
    Month = 2,
    Year = 3
}

/// <summary>
/// مرزهای بازه‌های آمار بر اساس تقویم شمسی: هفته از شنبه تا جمعه، ماه و سال شمسی (نه «۷/۳۰ روز اخیر»).
/// </summary>
public static class StatsPeriodRange
{
    private static readonly PersianCalendar Persian = new();

    public static (DateOnly From, DateOnly To) For(StatsPeriod period, DateOnly today)
    {
        switch (period)
        {
            case StatsPeriod.Week:
            {
                // DayOfWeek.Saturday = 6 → 0 روز عقب؛ Sunday = 0 → 1؛ ... Friday = 5 → 6
                var from = today.AddDays(-(((int)today.DayOfWeek + 1) % 7));
                return (from, from.AddDays(6));
            }
            case StatsPeriod.Month:
            {
                var (year, month) = PersianYearMonth(today);
                var from = FromPersian(year, month, 1);
                return (from, from.AddDays(Persian.GetDaysInMonth(year, month) - 1));
            }
            case StatsPeriod.Year:
            {
                var (year, _) = PersianYearMonth(today);
                var from = FromPersian(year, 1, 1);
                return (from, from.AddDays(Persian.GetDaysInYear(year) - 1));
            }
            default:
                return (today, today);
        }
    }

    /// <summary>
    /// بازه‌های ستون‌های نمودار: هفته/ماه روزبه‌روز، سال ماه‌به‌ماه (شمسی)، امروز بدون ستون.
    /// </summary>
    public static List<(DateOnly From, DateOnly To)> Buckets(StatsPeriod period, DateOnly from, DateOnly to)
    {
        var buckets = new List<(DateOnly From, DateOnly To)>();

        if (period is StatsPeriod.Week or StatsPeriod.Month)
        {
            for (var day = from; day <= to; day = day.AddDays(1))
                buckets.Add((day, day));
        }
        else if (period == StatsPeriod.Year)
        {
            var (year, _) = PersianYearMonth(from);
            for (var month = 1; month <= 12; month++)
            {
                var monthStart = FromPersian(year, month, 1);
                buckets.Add((monthStart, monthStart.AddDays(Persian.GetDaysInMonth(year, month) - 1)));
            }
        }

        return buckets;
    }

    private static (int Year, int Month) PersianYearMonth(DateOnly date)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);
        return (Persian.GetYear(dateTime), Persian.GetMonth(dateTime));
    }

    private static DateOnly FromPersian(int year, int month, int day) =>
        DateOnly.FromDateTime(Persian.ToDateTime(year, month, day, 0, 0, 0, 0));
}
