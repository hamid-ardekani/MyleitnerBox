using MyLeitner.Domain.Boxes;
using Should;

namespace CommandHandler_Test;

public class BacklogQuotaTest
{
    private static double[] Recent(int correct, int wrong) =>
        Enumerable.Repeat(1.0, correct).Concat(Enumerable.Repeat(0.0, wrong)).ToArray();

    [Fact]
    public void Accuracy_is_weighted_by_answer_quality_so_lucky_guesses_and_doubts_do_not_count_as_full()
    {
        // ده پاسخ «مطمئن نبودم»: هیچ اشتباهی نیست ولی کیفیت فقط ۵۰٪ → استراحت، نه رشد
        var doubts = BacklogQuota.Compute(20, 337, doneToday: 25, Enumerable.Repeat(0.5, 10).ToArray());
        doubts.Pace.ShouldEqual(BacklogPace.Resting);

        // ده پاسخ «نیاز به مرور»: ۷۵٪ → ثابت
        var reviews = BacklogQuota.Compute(20, 337, doneToday: 25, Enumerable.Repeat(0.75, 10).ToArray());
        reviews.Pace.ShouldEqual(BacklogPace.Steady);
    }

    [Fact]
    public void Before_ten_reviews_today_only_the_floor_is_released()
    {
        var q = BacklogQuota.Compute(floor: 20, remaining: 337, doneToday: 0, Recent(correct: 9, wrong: 0));

        q.Pace.ShouldEqual(BacklogPace.Warmup);
        q.Released.ShouldEqual(20);
        q.Available.ShouldEqual(20);
    }

    [Fact]
    public void Answering_well_does_not_open_more_by_itself_the_queue_runs_out_at_the_floor()
    {
        // ۱۵ از ۲۰ کفِ جعبه صاف شده، ۹ از ۱۰ درست: ۵ مانده - چیزی پشت سر کاربر اضافه نشده
        var q = BacklogQuota.Compute(20, 337, doneToday: 15, Recent(correct: 9, wrong: 1));
        q.Pace.ShouldEqual(BacklogPace.Growing);
        q.Released.ShouldEqual(20);
        q.Available.ShouldEqual(5);

        // کف تمام شده: چیزی در صف نمانده و ده کارت دیگر پیشنهاد می‌شود
        var done = BacklogQuota.Compute(20, 317, doneToday: 20, Recent(correct: 9, wrong: 1));
        done.Available.ShouldEqual(0);
        done.MoreAvailable.ShouldEqual(10);
    }

    [Fact]
    public void Accepting_the_offer_releases_exactly_that_many_more()
    {
        var q = BacklogQuota.Compute(20, 317, doneToday: 20, Recent(correct: 9, wrong: 1), bonus: 10);

        q.Released.ShouldEqual(30);
        q.Available.ShouldEqual(10);
        q.MoreAvailable.ShouldEqual(10); // and it can be offered again after those are done
    }

    [Fact]
    public void Answering_ok_offers_five()
    {
        var q = BacklogQuota.Compute(20, 337, doneToday: 20, Recent(correct: 7, wrong: 3));

        q.Pace.ShouldEqual(BacklogPace.Steady);
        q.MoreAvailable.ShouldEqual(5);
    }

    [Fact]
    public void Answering_badly_offers_nothing_but_the_floor_stays()
    {
        // اول روز: ۲۰ کارت کف حتی با سری بد هم هست (بدون قحطی)
        var early = BacklogQuota.Compute(20, 337, doneToday: 8, Recent(correct: 3, wrong: 7));
        early.Pace.ShouldEqual(BacklogPace.Resting);
        early.Available.ShouldEqual(12);

        // کف تمام شده: امروز چیز دیگری نیست و پیشنهادی هم نمی‌شود
        var late = BacklogQuota.Compute(20, 337, doneToday: 20, Recent(correct: 3, wrong: 7));
        late.Available.ShouldEqual(0);
        late.MoreAvailable.ShouldEqual(0);
    }

    [Fact]
    public void Never_more_than_eighty_a_day()
    {
        var q = BacklogQuota.Compute(20, 337, doneToday: 75, Recent(correct: 10, wrong: 0), bonus: 60);

        q.Pace.ShouldEqual(BacklogPace.Capped);
        q.Released.ShouldEqual(BacklogQuota.DailyCap);
        q.Available.ShouldEqual(5);
        q.MoreAvailable.ShouldEqual(0);
    }

    [Fact]
    public void Available_never_exceeds_what_is_left_and_nothing_is_offered_when_nothing_is_left()
    {
        var q = BacklogQuota.Compute(20, remaining: 7, doneToday: 0, []);

        q.Available.ShouldEqual(7);
        q.MoreAvailable.ShouldEqual(0);
    }

    [Fact]
    public void The_bonus_only_counts_on_its_own_day()
    {
        var today = new DateOnly(2026, 10, 18);

        Box.BacklogBonusFor(today, 10, today).ShouldEqual(10);
        Box.BacklogBonusFor(today.AddDays(-1), 10, today).ShouldEqual(0);
        Box.BacklogBonusFor(null, 0, today).ShouldEqual(0);
    }
}
