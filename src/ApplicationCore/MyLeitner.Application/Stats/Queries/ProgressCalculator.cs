using MyLeitner.Application.Stats.Queries.Models;
using MyLeitner.Domain.ReviewGoals;

namespace MyLeitner.Application.Stats.Queries;

/// <summary>
/// محاسبهٔ «پیشرفت من» از شمارش جواب‌های هر روز، بدون دیتابیس تا جداگانه تست شود.
/// مرز هفته و ماه همان <see cref="StatsPeriodRange"/> است.
/// </summary>
public static class ProgressCalculator
{
    public static ReviewGoalsDto DefaultGoals { get; } = new(ReviewGoal.DefaultDaily, ReviewGoal.DefaultWeekly, ReviewGoal.DefaultMonthly);

    /// <param name="perDay">
    /// امتیاز هر روز = جمع <c>AnswerQuality</c> جواب‌های آن روز (مطمئن ۱، با فکر ۰٫۷۵، شک ۰٫۵، شانسی و بلد نبودم ۰)، نه تعداد جواب.
    /// رکوردها و میانگین‌ها از همین حساب می‌شوند.
    /// </param>
    public static ProgressDto Compute(IReadOnlyDictionary<DateOnly, double> perDay, DateOnly today, ReviewGoalsDto goals)
    {
        int CountOn(DateOnly day) => Round(perDay.GetValueOrDefault(day));

        int Sum(DateOnly from, int days)
        {
            var total = 0.0;
            for (var i = 0; i < days; i++)
                total += perDay.GetValueOrDefault(from.AddDays(i));
            return Round(total);
        }

        // روزهای قبل از امروز که جواب داشتند: پایهٔ رکوردها و میانگین‌ها
        var past = perDay.Where(kv => kv.Key < today && Round(kv.Value) > 0).ToList();
        var bestDay = past.Count == 0 ? 0 : Round(past.Max(kv => kv.Value));

        // بیشترین امتیاز یک هفته/ماهِ تمام‌شده (دورهٔ جاری هنوز کامل نیست)
        int BestTotal(StatsPeriod period, DateOnly currentFrom) =>
            past.GroupBy(kv => StatsPeriodRange.For(period, kv.Key).From)
                .Where(g => g.Key != currentFrom)
                .Select(g => Round(g.Sum(kv => kv.Value)))
                .DefaultIfEmpty(0)
                .Max();

        // میانگین هر روز فعال از `from` (بدون امروز)
        double? Average(DateOnly from)
        {
            var days = past.Where(kv => kv.Key >= from).ToList();
            return days.Count == 0 ? null : Math.Round(days.Average(kv => kv.Value), 1);
        }

        var (weekFrom, weekTo) = StatsPeriodRange.For(StatsPeriod.Week, today);
        var weekDay = today.DayNumber - weekFrom.DayNumber + 1;
        var previousWeekFrom = weekFrom.AddDays(-7);
        var week = new ProgressSpanDto(
            Sum(weekFrom, weekDay),
            Sum(previousWeekFrom, weekDay),
            Sum(previousWeekFrom, 7),
            weekDay,
            weekTo.DayNumber - weekFrom.DayNumber + 1,
            BestTotal(StatsPeriod.Week, weekFrom));

        var (monthFrom, monthTo) = StatsPeriodRange.For(StatsPeriod.Month, today);
        var monthDay = today.DayNumber - monthFrom.DayNumber + 1;
        var (previousMonthFrom, previousMonthTo) = StatsPeriodRange.For(StatsPeriod.Month, monthFrom.AddDays(-1));
        var previousMonthDays = previousMonthTo.DayNumber - previousMonthFrom.DayNumber + 1;
        var month = new ProgressSpanDto(
            Sum(monthFrom, monthDay),
            Sum(previousMonthFrom, Math.Min(monthDay, previousMonthDays)),
            Sum(previousMonthFrom, previousMonthDays),
            monthDay,
            monthTo.DayNumber - monthFrom.DayNumber + 1,
            BestTotal(StatsPeriod.Month, monthFrom));

        var sameDateLastMonth = monthDay <= previousMonthDays ? CountOn(previousMonthFrom.AddDays(monthDay - 1)) : 0;
        var day = new ProgressDayDto(CountOn(today), CountOn(today.AddDays(-1)), CountOn(today.AddDays(-2)), CountOn(today.AddDays(-7)), bestDay, sameDateLastMonth);

        // میانگین سال به تاریخچهٔ کافی نیاز دارد: با کمتر از ۳۰ روز، حال‌وهوای امروز می‌شود نه میانگین
        var yearFrom = StatsPeriodRange.For(StatsPeriod.Year, today).From;
        var enoughHistory = past.Count > 0 && today.DayNumber - past.Min(kv => kv.Key).DayNumber >= 30;
        var averages = new ProgressAveragesDto(Average(today.AddDays(-7)), Average(today.AddDays(-30)), enoughHistory ? Average(yearFrom) : null);

        return new ProgressDto(today, goals, day, week, month, averages);
    }

    private static int Round(double points) => (int)Math.Round(points, MidpointRounding.AwayFromZero);
}
