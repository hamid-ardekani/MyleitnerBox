using MyLeitner.Application.Stats.Queries;
using Should;

namespace CommandHandler_Test;

public class StatsPeriodRangeTest
{
    // ۱۴۰۵/۰۷/۰۴ - شنبه
    private static readonly DateOnly Saturday = new(2026, 9, 26);

    [Fact]
    public void Week_starts_on_saturday()
    {
        var (from, to) = StatsPeriodRange.For(StatsPeriod.Week, Saturday);

        from.ShouldEqual(Saturday);
        to.ShouldEqual(new DateOnly(2026, 10, 2));
    }

    [Fact]
    public void Friday_belongs_to_the_week_that_started_the_saturday_before()
    {
        var (from, to) = StatsPeriodRange.For(StatsPeriod.Week, new DateOnly(2026, 9, 25));

        from.ShouldEqual(new DateOnly(2026, 9, 19));
        to.ShouldEqual(new DateOnly(2026, 9, 25));
    }

    [Fact]
    public void Month_is_the_persian_month()
    {
        // مهر ۱۴۰۵: ۱ مهر = ۲۳ سپتامبر، ۳۰ روز
        var (from, to) = StatsPeriodRange.For(StatsPeriod.Month, Saturday);

        from.ShouldEqual(new DateOnly(2026, 9, 23));
        to.ShouldEqual(new DateOnly(2026, 10, 22));
    }

    [Fact]
    public void Year_is_the_persian_year_with_twelve_month_buckets()
    {
        var (from, to) = StatsPeriodRange.For(StatsPeriod.Year, Saturday);

        from.ShouldEqual(new DateOnly(2026, 3, 21));
        to.ShouldEqual(new DateOnly(2027, 3, 20));

        var buckets = StatsPeriodRange.Buckets(StatsPeriod.Year, from, to);
        buckets.Count.ShouldEqual(12);
        buckets[0].ShouldEqual((new DateOnly(2026, 3, 21), new DateOnly(2026, 4, 20)));
        buckets[11].To.ShouldEqual(to);
    }

    [Fact]
    public void Week_has_a_bucket_per_day_and_today_has_none()
    {
        var (from, to) = StatsPeriodRange.For(StatsPeriod.Week, Saturday);

        StatsPeriodRange.Buckets(StatsPeriod.Week, from, to).Count.ShouldEqual(7);
        StatsPeriodRange.Buckets(StatsPeriod.Today, Saturday, Saturday).Count.ShouldEqual(0);
    }
}
