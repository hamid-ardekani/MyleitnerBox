using Autofac;
using CommandHandler_Test.Autofac;
using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Dtos;
using MyLeitner.Application.Learnings.Enums;
using MyLeitner.Application.Learnings.Queries;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.LearningSchedules.Exceptions;
using Should;

namespace CommandHandler_Test;

public class AnswerPreviewTest : IClassFixture<AutofacTestFixture>
{
    private const long UserId = 1;
    private const int Median = LearningSchedule.DefaultUserGlobalMedianSeconds;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private readonly IBoxRepository _boxRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly ILearningScheduleRepository _scheduleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryDispatcher _queryDispatcher;

    public AnswerPreviewTest(AutofacTestFixture fixture)
    {
        var scope = fixture.Container.BeginLifetimeScope();
        _boxRepository = scope.Resolve<IBoxRepository>();
        _flashcardRepository = scope.Resolve<IFlashcardRepository>();
        _scheduleRepository = scope.Resolve<ILearningScheduleRepository>();
        _unitOfWork = scope.Resolve<IUnitOfWork>();
        _queryDispatcher = scope.Resolve<IQueryDispatcher>();
    }

    private static LearningSchedule ScheduleAfter(params (AnswerRateTimeType Type, int Seconds)[] history)
    {
        var schedule = LearningSchedule.ForAdd(UserId, 1, 1);
        foreach (var (type, seconds) in history)
            schedule.AnswerProcess(AnswerTypeEnum.Create((short)type), seconds, Median);
        return schedule;
    }

    public static IEnumerable<object[]> Histories()
    {
        yield return [Array.Empty<(AnswerRateTimeType, int)>()];
        yield return [new[] { (AnswerRateTimeType.Confident, 4) }];
        yield return [new[] { (AnswerRateTimeType.Confident, 4), (AnswerRateTimeType.NotSure, 30), (AnswerRateTimeType.Confident, 200) }];
        yield return [new[] { (AnswerRateTimeType.Wrong, 10), (AnswerRateTimeType.NeedMoreReview, 90) }];
    }

    [Theory]
    [MemberData(nameof(Histories))]
    public void PreviewIntervalDays_EqualsTheIntervalAnswerProcessProduces_ForEveryAnswerType(
        (AnswerRateTimeType Type, int Seconds)[] history)
    {
        foreach (var type in Enum.GetValues<AnswerRateTimeType>())
        foreach (var seconds in new[] { 2, 60, 500 })
        {
            var forPreview = ScheduleAfter(history);
            var forAnswer = ScheduleAfter(history);

            var previewed = forPreview.PreviewIntervalDays(AnswerTypeEnum.Create((short)type), seconds, Median);
            forAnswer.AnswerProcess(AnswerTypeEnum.Create((short)type), seconds, Median);

            previewed.ShouldEqual((int)forAnswer.Interval);
        }
    }

    [Fact]
    public void PreviewIntervalDays_ChangesNothingOnTheSchedule()
    {
        var schedule = ScheduleAfter((AnswerRateTimeType.Confident, 5));
        var (stability, difficulty, interval, next) = (schedule.Stability, schedule.Difficulty, schedule.Interval, schedule.NextAnswerDate);

        schedule.PreviewIntervalDays(AnswerTypeEnum.Create((short)AnswerRateTimeType.Wrong), 5, Median);

        schedule.Stability.ShouldEqual(stability);
        schedule.Difficulty.ShouldEqual(difficulty);
        schedule.Interval.ShouldEqual(interval);
        schedule.NextAnswerDate.ShouldEqual(next);
    }

    [Fact]
    public async Task Handle_ReturnsOneIntervalPerAnswerType_AtLeastOneDay_ConfidentNotShorterThanWrong()
    {
        var box = Box.BoxForAdd("preview box", 1, false, UserId);
        await _boxRepository.AddAsync(box);
        await _unitOfWork.CommitAsync(_cancellationToken);

        var flashcard = Flashcard.ForAdd(box.Id, 1, "front", "back", "front", "back", ["tag"]);
        await _flashcardRepository.AddAsync(flashcard);
        await _unitOfWork.CommitAsync(_cancellationToken);

        // دستور واقعی افزودن، schedule را با رویداد دامنه می‌سازد؛ seed از طریق repository این کار را نمی‌کند
        await _scheduleRepository.AddAsync(LearningSchedule.ForAdd(UserId, box.Id, flashcard.Id));
        await _unitOfWork.CommitAsync(_cancellationToken);

        var preview = await _queryDispatcher.Dispatch<GetAnswerPreviewQuery, List<AnswerIntervalPreviewDto>>(
            new GetAnswerPreviewQuery(flashcard.Id, 5, UserId), _cancellationToken);

        preview.Select(p => p.AnswerRateTimeType).OrderBy(x => x)
            .ShouldEqual(Enum.GetValues<AnswerRateTimeType>().Select(t => (int)t).OrderBy(x => x));
        preview.All(p => p.IntervalDays >= 1).ShouldBeTrue();

        var confident = preview.Single(p => p.AnswerRateTimeType == (int)AnswerRateTimeType.Confident).IntervalDays;
        var wrong = preview.Single(p => p.AnswerRateTimeType == (int)AnswerRateTimeType.Wrong).IntervalDays;
        (confident >= wrong).ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Throws_WhenTheFlashcardHasNoSchedule()
    {
        await Assert.ThrowsAsync<LearningScheduleNotFoundException>(() =>
            _queryDispatcher.Dispatch<GetAnswerPreviewQuery, List<AnswerIntervalPreviewDto>>(
                new GetAnswerPreviewQuery(long.MaxValue, 5, UserId), _cancellationToken));
    }
}
