using Autofac;
using CommandHandler_Test.Autofac;
using Dela.Toolkit.Application;
using MyLeitner.Application.Contract.Dtos;
using MyLeitner.Application.Learnings.Queries;
using MyLeitner.Common.Results;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;
using MyLeitner.Persistence.ReadModel.Sql.ReadModels;
using Should;

namespace CommandHandler_Test;

public class GetNextPlayFlashcardInCategoryTest : IClassFixture<AutofacTestFixture>
{
    private const long UserId = 1;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly ReadModelDbContext _readContext;

    public GetNextPlayFlashcardInCategoryTest(AutofacTestFixture fixture)
    {
        var scope = fixture.Container.BeginLifetimeScope();
        _queryDispatcher = scope.Resolve<IQueryDispatcher>();
        _readContext = scope.Resolve<ReadModelDbContext>();
    }

    [Fact]
    public async Task Handle_ReturnsHasFlashcards_WhenADueCardExists()
    {
        var box = new Box { Name = "play box", OrderBox = 1, ReadFlashcardAloud = false, UserId = UserId, DailyNewCardLimit = 10, DailyBacklogFloor = 20 };
        _readContext.Boxes.Add(box);
        await _readContext.SaveChangesAsync(_cancellationToken);

        var category = new Category { Name = "play category", Order = 1, UserId = UserId };
        _readContext.Categories.Add(category);
        await _readContext.SaveChangesAsync(_cancellationToken);

        _readContext.CategoryBoxes.Add(new CategoryBox { Order = 1, BoxId = box.Id, CategoryId = category.Id });

        var flashcard = new Flashcard { Order = 1, Front = "play front", Back = "play back", Tags = "t", BoxId = box.Id };
        _readContext.Flashcards.Add(flashcard);
        await _readContext.SaveChangesAsync(_cancellationToken);

        _readContext.LearningSchedules.Add(new LearningSchedule
        {
            FlashcardId = flashcard.Id, UserId = UserId, Stability = 1, Difficulty = 0.3, Interval = 1, NextAnswerDate = null
        });
        await _readContext.SaveChangesAsync(_cancellationToken);

        var result = await _queryDispatcher.Dispatch<GetNextPlayFlashcardInCategoryQuery, Result<StudySessionResult>>(
            new GetNextPlayFlashcardInCategoryQuery(category.Id, UserId), _cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldEqual(LearningScheduleStatus.HasFlashcards);
        result.Value.Flashcard.ShouldNotBeNull();
        result.Value.Flashcard!.Front.ShouldEqual("play front");
    }

    [Fact]
    public async Task Handle_ReturnsAllCompleted_WhenCategoryHasNoDueCards()
    {
        var category = new Category { Name = "empty category", Order = 1, UserId = UserId };
        _readContext.Categories.Add(category);
        await _readContext.SaveChangesAsync(_cancellationToken);

        var result = await _queryDispatcher.Dispatch<GetNextPlayFlashcardInCategoryQuery, Result<StudySessionResult>>(
            new GetNextPlayFlashcardInCategoryQuery(category.Id, UserId), _cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldEqual(LearningScheduleStatus.AllFlashcardsCompletedForToday);
        result.Value.Flashcard.ShouldBeNull();
    }
}
