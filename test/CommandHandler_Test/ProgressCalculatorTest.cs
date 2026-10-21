using MyLeitner.Application.Stats.Queries;
using Should;

namespace CommandHandler_Test;

public class ProgressCalculatorTest
{
    // ۱۴۰۵/۰۷/۲۰ - دوشنبه، سومین روز هفته (شنبه ۱۰ مهر) و بیستمین روز مهر
    private static readonly DateOnly Monday = new(2026, 10, 12);

    private static Dictionary<DateOnly, double> Counts(params (DateOnly Day, double Count)[] items) =>
        items.ToDictionary(i => i.Day, i => i.Count);

    [Fact]
    public void Day_is_compared_with_yesterday_day_before_and_same_day_last_week()
    {
        var perDay = Counts((Monday, 12), (Monday.AddDays(-1), 9), (Monday.AddDays(-2), 4), (Monday.AddDays(-7), 15));

        var progress = ProgressCalculator.Compute(perDay, Monday, ProgressCalculator.DefaultGoals);

        progress.Day.Today.ShouldEqual(12);
        progress.Day.Yesterday.ShouldEqual(9);
        progress.Day.DayBefore.ShouldEqual(4);
        progress.Day.SameDayLastWeek.ShouldEqual(15);
        progress.Day.BestDay.ShouldEqual(15); // the best of the earlier days (today's 12 is what is compared with it)
    }

    [Fact]
    public void Best_day_is_the_most_answers_on_any_earlier_day()
    {
        var progress = ProgressCalculator.Compute(Counts((Monday, 99), (Monday.AddDays(-30), 61), (Monday.AddDays(-3), 20)), Monday, ProgressCalculator.DefaultGoals);

        progress.Day.BestDay.ShouldEqual(61); // today's 99 is not the record yet - it is what gets compared with it
    }

    [Fact]
    public void Same_date_last_month_is_the_same_day_number_of_the_previous_persian_month()
    {
        // امروز ۲۰ مهر ← ۲۰ شهریور = ۱۱ سپتامبر
        var perDay = Counts((new DateOnly(2026, 9, 11), 33), (new DateOnly(2026, 9, 12), 5));

        ProgressCalculator.Compute(perDay, Monday, ProgressCalculator.DefaultGoals).Day.SameDateLastMonth.ShouldEqual(33);
    }

    [Fact]
    public void Averages_count_only_active_days_and_leave_today_out()
    {
        // ۷ روز اخیر: ۱۰ و ۳۰ (دو روز فعال، بقیه خالی) ← میانگین ۲۰؛ امروز ۱۰۰ حساب نمی‌شود
        var perDay = Counts((Monday, 100), (Monday.AddDays(-1), 10), (Monday.AddDays(-4), 30));

        var averages = ProgressCalculator.Compute(perDay, Monday, ProgressCalculator.DefaultGoals).Averages;

        averages.Week.ShouldEqual(20.0);
        averages.Month.ShouldEqual(20.0);
    }

    [Fact]
    public void Year_average_waits_for_thirty_days_of_history()
    {
        var shortHistory = Counts((Monday.AddDays(-10), 40), (Monday.AddDays(-1), 20));
        ProgressCalculator.Compute(shortHistory, Monday, ProgressCalculator.DefaultGoals).Averages.Year.ShouldBeNull();

        var longHistory = Counts((Monday.AddDays(-31), 40), (Monday.AddDays(-1), 20));
        ProgressCalculator.Compute(longHistory, Monday, ProgressCalculator.DefaultGoals).Averages.Year.ShouldEqual(30.0);
    }

    [Fact]
    public void Best_week_total_leaves_out_the_week_in_progress()
    {
        // هفتهٔ پیش (شنبه ۱۲ مهر تا جمعه ۱۸ مهر): ۴۰ + ۶۰ = ۱۰۰؛ این هفته ۹۹۹ است ولی هنوز تمام نشده
        var perDay = Counts((new DateOnly(2026, 10, 10), 999), (new DateOnly(2026, 10, 3), 40), (new DateOnly(2026, 10, 4), 60));

        ProgressCalculator.Compute(perDay, Monday, ProgressCalculator.DefaultGoals).Week.BestTotal.ShouldEqual(100);
    }

    [Fact]
    public void Week_is_compared_with_last_week_up_to_the_same_day()
    {
        // این هفته: شنبه ۳، یکشنبه ۴، دوشنبه (امروز) ۵ - هفتهٔ قبل: ۲ و ۶ و ۱ و ۹
        var perDay = Counts(
            (new DateOnly(2026, 10, 10), 3), (new DateOnly(2026, 10, 11), 4), (Monday, 5),
            (new DateOnly(2026, 10, 3), 2), (new DateOnly(2026, 10, 4), 6), (new DateOnly(2026, 10, 5), 1), (new DateOnly(2026, 10, 6), 9));

        var week = ProgressCalculator.Compute(perDay, Monday, ProgressCalculator.DefaultGoals).Week;

        week.Count.ShouldEqual(12);
        week.DayNumber.ShouldEqual(3);
        week.TotalDays.ShouldEqual(7);
        week.PreviousSamePoint.ShouldEqual(9);
        week.PreviousTotal.ShouldEqual(18);
    }

    [Fact]
    public void Month_is_compared_with_the_previous_persian_month_up_to_the_same_day()
    {
        // امروز بیستم مهر؛ شهریور ۳۱ روز دارد و از ۱ شهریور (۲۳ اوت) شروع می‌شود
        var perDay = Counts(
            (new DateOnly(2026, 10, 1), 30), (Monday, 5),
            (new DateOnly(2026, 8, 23), 7), (new DateOnly(2026, 9, 11), 3), (new DateOnly(2026, 9, 12), 100));

        var month = ProgressCalculator.Compute(perDay, Monday, ProgressCalculator.DefaultGoals).Month;

        month.Count.ShouldEqual(35);
        month.DayNumber.ShouldEqual(20);
        month.TotalDays.ShouldEqual(30);
        month.PreviousSamePoint.ShouldEqual(10);
        month.PreviousTotal.ShouldEqual(110);
    }

    [Fact]
    public void Points_are_summed_then_rounded()
    {
        // امروز: ۲ مطمئن + ۱ با فکر (۲٫۷۵ ← ۳)؛ دیروز: ۱۰۰ شانسی = ۰ امتیاز
        var perDay = Counts((Monday, 2.75), (Monday.AddDays(-1), 0));

        var progress = ProgressCalculator.Compute(perDay, Monday, ProgressCalculator.DefaultGoals);

        progress.Day.Today.ShouldEqual(3);
        progress.Day.Yesterday.ShouldEqual(0);
        progress.Week.Count.ShouldEqual(3);
    }

    [Fact]
    public void Defaults_are_10_100_400()
    {
        ProgressCalculator.DefaultGoals.Daily.ShouldEqual(10);
        ProgressCalculator.DefaultGoals.Weekly.ShouldEqual(100);
        ProgressCalculator.DefaultGoals.Monthly.ShouldEqual(400);
    }
}
