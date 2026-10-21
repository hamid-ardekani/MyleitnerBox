using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Boxes.Exceptions;
using Should;

namespace CommandHandler_Test;

public class BoxDailyNewCardLimitTest
{
    [Fact]
    public void New_box_gets_the_default_limit()
    {
        Box.BoxForAdd("b", 1, false, 1).DailyNewCardLimit.ShouldEqual(Box.DefaultDailyNewCardLimit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Limit_outside_1_to_99_is_rejected(short limit)
    {
        Assert.Throws<DailyNewFlashcardCountUnValidException>(() => Box.BoxForAdd("b", 1, false, 1, limit));
    }

    [Fact]
    public void Update_without_a_limit_keeps_the_current_one()
    {
        var box = Box.BoxForAdd("b", 1, false, 1, 25);
        box.Update("renamed", true);
        box.DailyNewCardLimit.ShouldEqual((short)25);
    }

    private static NewCardQuota Quota(int dueReviews = 0, int todayWrong = 0, int started = 0, int recentReviews = 0, int recentCorrect = 0, short limit = 10) =>
        Box.NewCardQuotaToday(limit, dueReviews, todayWrong, started, recentReviews, recentCorrect);

    [Fact]
    public void Allowed_is_the_target_minus_cards_already_started_today()
    {
        Quota(started: 4).Allowed.ShouldEqual(6);
        Quota(started: 12).Allowed.ShouldEqual(0);
    }

    [Theory]
    [InlineData(10, 10)] // up to 10 due reviews: full limit
    [InlineData(15, 5)]  // 10..20: linear
    [InlineData(19, 1)]
    [InlineData(20, 0)]  // 20 and more: no new cards
    [InlineData(25, 0)]
    public void Workload_scales_new_cards_down_gradually(int dueReviews, int target)
    {
        Quota(dueReviews: dueReviews).Target.ShouldEqual(target);
    }

    [Theory]
    [InlineData(20, 19, 12)] // 95% -> +20%
    [InlineData(20, 18, 12)] // 90% -> +20%
    [InlineData(20, 15, 10)] // 75% -> unchanged
    [InlineData(20, 13, 7)]  // 65% -> -30%
    [InlineData(19, 5, 10)]  // fewer than 20 reviews: not enough data, neutral
    public void Recent_review_accuracy_rewards_or_slows_new_cards(int recentReviews, int recentCorrect, int target)
    {
        Quota(recentReviews: recentReviews, recentCorrect: recentCorrect).Target.ShouldEqual(target);
    }

    [Fact]
    public void Workload_and_accuracy_combine_like_the_agreed_examples()
    {
        // ۱۵ مرور موعدرسیده با دقت ۹۳٪: 10 × 0.5 × 1.2 = 6
        Quota(dueReviews: 15, recentReviews: 100, recentCorrect: 93).Target.ShouldEqual(6);
    }

    [Fact]
    public void More_than_five_wrong_today_stops_new_cards_whatever_else()
    {
        var quota = Quota(todayWrong: 6, recentReviews: 100, recentCorrect: 100);
        quota.StoppedByWrongs.ShouldBeTrue();
        quota.Allowed.ShouldEqual(0);
        Quota(todayWrong: 5).Allowed.ShouldEqual(10);
    }

    [Fact]
    public void Bonus_never_goes_past_99()
    {
        Quota(limit: 99, recentReviews: 100, recentCorrect: 100).Target.ShouldEqual(99);
    }
}
