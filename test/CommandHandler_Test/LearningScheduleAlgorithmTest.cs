using MyLeitner.Application.Learnings.Enums;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.LearningSchedules;
using Should;

namespace CommandHandler_Test;

public class LearningScheduleAlgorithmTest
{
    private const int Median = LearningSchedule.DefaultUserGlobalMedianSeconds;

    private static readonly AnswerRateTimeType[] WorstToBest =
    [
        AnswerRateTimeType.Wrong,
        AnswerRateTimeType.UnCertain,
        AnswerRateTimeType.NotSure,
        AnswerRateTimeType.NeedMoreReview,
        AnswerRateTimeType.Confident,
    ];

    private static AnswerTypeEnum Type(AnswerRateTimeType type) => AnswerTypeEnum.Create((short)type);

    private static LearningSchedule NewCard() => LearningSchedule.ForAdd(1, 1, 1);

    private static void Answer(LearningSchedule schedule, AnswerRateTimeType type, int seconds = 60)
        => schedule.AnswerProcess(Type(type), seconds, Median);

    [Theory]
    [InlineData(2)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    [InlineData(500)]
    public void FirstReview_WrongAndGuessComeBackTomorrow_TheRestLaterTheSurerYouWere_AtAnySpeed(int seconds)
    {
        var days = WorstToBest.Select(type => NewCard().PreviewIntervalDays(Type(type), seconds, Median)).ToArray();

        days[0].ShouldEqual(1); // Wrong
        days[1].ShouldEqual(1); // UnCertain: a lucky guess halves the interval, and a new card is already at the 1-day floor
        for (var i = 2; i < days.Length; i++)
            (days[i] > days[i - 1]).ShouldBeTrue($"{WorstToBest[i]} ({days[i]}d) should come back later than {WorstToBest[i - 1]} ({days[i - 1]}d): {string.Join(",", days)}");
    }

    [Fact]
    public void FirstReview_ConfidentAnswerLandsAroundAWeek_NotJustTwoDays()
    {
        var days = NewCard().PreviewIntervalDays(Type(AnswerRateTimeType.Confident), 60, Median);

        days.ShouldBeInRange(5, 7);
    }

    [Fact]
    public void Wrong_OnAMatureCard_SendsItBackToOneDay_AndItRegrowsFromThere()
    {
        var schedule = NewCard();
        for (var i = 0; i < 4; i++) Answer(schedule, AnswerRateTimeType.Confident);
        var matureInterval = schedule.Interval;
        var difficultyBefore = schedule.Difficulty;
        (matureInterval >= 20).ShouldBeTrue();

        Answer(schedule, AnswerRateTimeType.Wrong);

        schedule.Interval.ShouldEqual(1);
        (schedule.Difficulty > difficultyBefore).ShouldBeTrue();

        Answer(schedule, AnswerRateTimeType.Confident);
        (schedule.Interval > 1).ShouldBeTrue();
        (schedule.Interval < matureInterval).ShouldBeTrue();
    }

    /// <summary>«شانسی» فاصله را نصف و کارت را سخت‌تر می‌کند؛ «شک» فقط ۱۵٪ رشد می‌دهد و سختی را دست نمی‌زند.</summary>
    [Fact]
    public void Guess_HalvesTheInterval_AndNotSure_BarelyGrowsIt_OnAMatureCard()
    {
        var schedule = NewCard();
        for (var i = 0; i < 4; i++) Answer(schedule, AnswerRateTimeType.Confident);
        var stability = schedule.Stability;
        var difficulty = schedule.Difficulty;
        (stability >= 20).ShouldBeTrue();

        var guess = schedule.PreviewIntervalDays(Type(AnswerRateTimeType.UnCertain), 60, Median);
        var notSure = schedule.PreviewIntervalDays(Type(AnswerRateTimeType.NotSure), 60, Median);

        ((double)guess).ShouldBeInRange(Math.Round(stability * 0.5) - 1, Math.Round(stability * 0.5) + 1);
        (notSure > stability).ShouldBeTrue();
        ((double)notSure).ShouldBeInRange(stability, stability * 1.2);

        Answer(schedule, AnswerRateTimeType.UnCertain);
        schedule.Difficulty.ShouldEqual(difficulty + 0.1, 1e-9);
    }

    [Fact]
    public void NotSure_LeavesDifficultyUnchanged()
    {
        var schedule = NewCard();
        Answer(schedule, AnswerRateTimeType.Confident);
        var difficulty = schedule.Difficulty;

        Answer(schedule, AnswerRateTimeType.NotSure);

        schedule.Difficulty.ShouldEqual(difficulty, 1e-9);
    }

    [Fact]
    public void Guess_AtTheCap_DropsTheCardInsteadOfKeepingIt()
    {
        var schedule = NewCard();
        for (var i = 0; i < 100 && schedule.Stability < LearningSchedule.MaxStability; i++) Answer(schedule, AnswerRateTimeType.Confident, 10);

        schedule.PreviewIntervalDays(Type(AnswerRateTimeType.UnCertain), 60, Median).ShouldEqual(90);
    }

    [Fact]
    public void RepeatedConfidentAnswers_GrowEachTime_AndStopAtTheCap()
    {
        var schedule = NewCard();
        var previous = 0d;

        for (var i = 0; i < 12; i++)
        {
            Answer(schedule, AnswerRateTimeType.Confident);
            (schedule.Interval >= previous).ShouldBeTrue();
            (schedule.Interval <= 180).ShouldBeTrue();
            previous = schedule.Interval;
        }

        previous.ShouldEqual(180);
    }

    [Fact]
    public void EarlyBonus_FadesAsTheCardMatures_SoLaterGrowthIsMostlyMultiplicative()
    {
        var schedule = NewCard();
        for (var i = 0; i < 3; i++) Answer(schedule, AnswerRateTimeType.Confident);
        var stabilityBefore = schedule.Stability;

        var next = schedule.PreviewIntervalDays(Type(AnswerRateTimeType.Confident), 60, Median);

        // در کارت پخته، جملهٔ کمکی کمتر از یک روز است، پس فاصله فقط همان ضریب رشد است.
        ((double)next / stabilityBefore).ShouldBeInRange(2.0, 2.6);
    }
}
