using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.LearningSchedules;
using Should;

namespace CommandHandler_Test;

public class LearningStatusTest
{
    private const int Seconds = 10;
    private const int Median = LearningSchedule.DefaultUserGlobalMedianSeconds;

    /// <summary>«مطمئن» پشت‌سرهم تا کارت لبهٔ پایان باشد: «مطمئن»ِ بعدی فاصله را به سقف می‌رساند (هنوز یادگرفته‌شده نشده).</summary>
    private static LearningSchedule AtMaxStability()
    {
        var schedule = LearningSchedule.ForAdd(1, 1, 1);
        for (var i = 0; i < 100 && !schedule.WouldMaster(AnswerTypeEnum.Confident, Seconds, Median); i++)
            schedule.AnswerProcess(AnswerTypeEnum.Confident, Seconds, Median);

        schedule.WouldMaster(AnswerTypeEnum.Confident, Seconds, Median).ShouldBeTrue();
        schedule.Status.ShouldEqual(LearningStatus.Learning);
        return schedule;
    }

    [Fact]
    public void Confident_answer_that_reaches_the_max_interval_masters_the_card()
    {
        var schedule = AtMaxStability();

        schedule.WouldMaster(AnswerTypeEnum.Confident, Seconds, Median).ShouldBeTrue();
        schedule.AnswerProcess(AnswerTypeEnum.Confident, Seconds, Median);

        schedule.Status.ShouldEqual(LearningStatus.Mastered);
        schedule.StatusChangedAt.HasValue.ShouldBeTrue();
    }

    [Theory]
    [InlineData(1)] // NeedMoreReview
    [InlineData(2)] // NotSure
    [InlineData(3)] // UnCertain
    [InlineData(4)] // Wrong
    public void Any_other_answer_on_the_edge_of_mastery_keeps_it_learning(short answerTypeId)
    {
        var schedule = AtMaxStability();
        var answer = AnswerTypeEnum.Create(answerTypeId);

        schedule.WouldMaster(answer, Seconds, Median).ShouldBeFalse();
        schedule.AnswerProcess(answer, Seconds, Median);

        schedule.Status.ShouldEqual(LearningStatus.Learning);
    }

    [Fact]
    public void Archive_then_restore_brings_the_card_back_due_now()
    {
        var schedule = AtMaxStability();

        schedule.Archive();
        schedule.Status.ShouldEqual(LearningStatus.Archived);

        schedule.RestoreToLearning();
        schedule.Status.ShouldEqual(LearningStatus.Learning);
        (schedule.NextAnswerDate <= DateTime.UtcNow).ShouldBeTrue();
    }

    [Fact]
    public void Restoring_a_never_answered_card_keeps_it_new()
    {
        var schedule = LearningSchedule.ForAdd(1, 1, 1);

        schedule.Archive();
        schedule.RestoreToLearning();

        schedule.Status.ShouldEqual(LearningStatus.Learning);
        schedule.NextAnswerDate.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Known_new_card_leaves_review_as_mastered_and_restores_as_new()
    {
        var schedule = LearningSchedule.ForAdd(1, 1, 1);

        schedule.MarkKnown();

        schedule.Status.ShouldEqual(LearningStatus.Mastered);
        schedule.StatusChangedAt.HasValue.ShouldBeTrue();
        schedule.LastAnswerDate.HasValue.ShouldBeFalse(); // not an answer

        schedule.RestoreToLearning();
        schedule.Status.ShouldEqual(LearningStatus.Learning);
        schedule.NextAnswerDate.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Known_card_in_review_keeps_its_interval()
    {
        var schedule = LearningSchedule.ForAdd(1, 1, 1);
        schedule.AnswerProcess(AnswerTypeEnum.Confident, Seconds, Median);
        var interval = schedule.Interval;

        schedule.MarkKnown();

        schedule.Status.ShouldEqual(LearningStatus.Mastered);
        schedule.Interval.ShouldEqual(interval);
    }

    [Fact]
    public void A_new_card_can_be_added_to_review_today()
    {
        var schedule = LearningSchedule.ForAdd(1, 1, 1);

        schedule.AddToReview();

        schedule.AddedToReviewOn.HasValue.ShouldBeTrue();
    }

    [Fact]
    public void Adding_to_review_does_nothing_to_a_card_that_was_already_answered_or_is_out_of_review()
    {
        var answered = LearningSchedule.ForAdd(1, 1, 1);
        answered.AnswerProcess(AnswerTypeEnum.Confident, Seconds, Median);
        answered.AddToReview();
        answered.AddedToReviewOn.HasValue.ShouldBeFalse();

        var archived = LearningSchedule.ForAdd(1, 1, 2);
        archived.Archive();
        archived.AddToReview();
        archived.AddedToReviewOn.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void A_confident_answer_that_does_not_reach_the_cap_keeps_the_card_in_review()
    {
        var schedule = LearningSchedule.ForAdd(1, 1, 1);

        schedule.WouldMaster(AnswerTypeEnum.Confident, Seconds, Median).ShouldBeFalse(); // a new card
        schedule.AnswerProcess(AnswerTypeEnum.Confident, Seconds, Median);

        schedule.Status.ShouldEqual(LearningStatus.Learning);
    }

    [Fact]
    public void The_preview_of_a_confident_answer_that_reaches_the_cap_says_the_card_is_learned()
    {
        // پاسخی که «یادگرفته‌شده» نشان داده می‌شود همان است که واقعاً کارت را یاد می‌گیرد
        var schedule = AtMaxStability();
        var previewMasters = schedule.WouldMaster(AnswerTypeEnum.Confident, Seconds, Median);

        schedule.AnswerProcess(AnswerTypeEnum.Confident, Seconds, Median);

        previewMasters.ShouldBeTrue();
        schedule.Status.ShouldEqual(LearningStatus.Mastered);
    }
}
