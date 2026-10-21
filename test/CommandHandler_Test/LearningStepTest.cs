using MyLeitner.Domain.LearningSchedules;
using Should;

namespace CommandHandler_Test;

public class LearningStepTest
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(1.9, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 2)]
    [InlineData(4, 3)]
    [InlineData(7, 3)]
    [InlineData(8, 4)]
    [InlineData(15, 4)]
    [InlineData(16, 5)]
    [InlineData(32, 6)]
    [InlineData(64, 7)]
    [InlineData(127, 7)]
    [InlineData(128, 8)]
    [InlineData(180, 8)]
    public void Interval_maps_to_doubling_steps(double intervalDays, int step)
    {
        LearningStep.ForInterval(intervalDays).ShouldEqual(step);
    }

    [Fact]
    public void Never_answered_is_new_and_mastered_is_its_own_step()
    {
        LearningStep.For(neverAnswered: true, intervalDays: 0).ShouldEqual(LearningStep.New);
        LearningStep.For(neverAnswered: false, intervalDays: 180, LearningStatus.Mastered).ShouldEqual(LearningStep.Mastered);
    }
}
