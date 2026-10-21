using Autofac;
using CommandHandler_Test.Autofac;
using Dela.Toolkit.Application;
using Microsoft.EntityFrameworkCore;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Domain.Flashcards.Exceptions;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Persistence.WriteModel;
using Should;

namespace CommandHandler_Test;

public class FlashcardTest : IClassFixture<AutofacTestFixture>
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private readonly ILearningScheduleRepository _learningScheduleRepository;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly MyLeitnerDbContext _context;

    public FlashcardTest(AutofacTestFixture fixture)
    {
        var scope = fixture.Container.BeginLifetimeScope();
        _context = scope.Resolve<MyLeitnerDbContext>();

        _learningScheduleRepository = scope.Resolve<ILearningScheduleRepository>();
        _commandDispatcher = scope.Resolve<ICommandDispatcher>();
    }

    [Fact]
    public async Task Add_Flashcard_learningSchedule()
    {
        await Task.Delay(100,_cancellationToken);

        await _commandDispatcher.Dispatch(new AddBoxCommand("box 1", true, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, "Front 1", "Back 1", "Front 1", "Back 1", ["Tag1", "Tag2", "Tag3"], 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, "Front 2", "Back 2", "Front 2", "Back 2", ["Tag4", "Tag22", "Tag32"], 1), _cancellationToken);


        _context.Flashcards.Count().ShouldEqual(2);

        var firstItem = await _context.Flashcards.FirstOrDefaultAsync(c => c.Id == 1, _cancellationToken);
        firstItem.ShouldNotBeNull();
        firstItem!.Front.ShouldEqual("Front 1");
        firstItem.Back.ShouldEqual("Back 1");
        firstItem.Order.ShouldEqual((short)1);
        firstItem.Tags.ShouldEqual("Tag1,Tag2,Tag3");

        var secondItem = await _context.Flashcards.FirstOrDefaultAsync(c => c.Id == 2, _cancellationToken);
        secondItem.ShouldNotBeNull();
        secondItem!.Front.ShouldEqual("Front 2");
        secondItem.Back.ShouldEqual("Back 2");
        secondItem.Order.ShouldEqual((short)2);
        secondItem.Tags.ShouldEqual("Tag4,Tag22,Tag32");

        var learning = await _learningScheduleRepository.GetByIdAsync(1, _cancellationToken);
        learning.ShouldNotBeNull();
        learning!.Id.ShouldEqual(1);
        learning.FlashcardId.ShouldEqual(1);


        var secondLearning = await _learningScheduleRepository.GetByIdAsync(2, _cancellationToken);
        secondLearning.ShouldNotBeNull();
        secondLearning!.Id.ShouldEqual(2);
        secondLearning.FlashcardId.ShouldEqual(2);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public async Task Add_Front_Empty_Flashcard(string? front)
    {
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 1", true, 1), _cancellationToken);

        try
        {
            await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, front, "Back 1", front, "Back 1", ["Tag1", "Tag2", "Tag3"], 1), _cancellationToken);
            0.ShouldBeNull();
        }
        catch (Exception e)
        {
            e.GetType().ShouldEqual(typeof(FrontNullException));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public async Task Add_Back_Null_Flashcard(string? back)
    {
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 1", true, 1), _cancellationToken);

        try
        {
            await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, "Front", back, "Front", back, ["Tag1", "Tag2", "Tag3"], 1), _cancellationToken);
            0.ShouldBeNull();
        }
        catch (Exception e)
        {
            e.GetType().ShouldEqual(typeof(BackNullException));
        }
    }

    [Fact]
    public async Task Update_Flashcard()
    {
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 1", true, 1), _cancellationToken);

        await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, "Front 1", "Back 1", "Front 1", "Back 1", ["Tag1", "Tag2", "Tag3"], 1), _cancellationToken);

        await _commandDispatcher.Dispatch(new UpdateFlashcardCommand(1, "Front first", "Back first", "Front first", "Back first", ["TagF1", "Tag2", "TagF3"], 1), _cancellationToken);

        _context.Flashcards.Count().ShouldEqual(1);

        var firstItem = await _context.Flashcards.FirstOrDefaultAsync(c => c.Id == 1, _cancellationToken);
        firstItem.ShouldNotBeNull();
        firstItem!.Front.ShouldEqual("Front first");
        firstItem.Back.ShouldEqual("Back first");
        firstItem.Order.ShouldEqual((short)1);
        firstItem.Tags.ShouldEqual("TagF1,Tag2,TagF3");
    }

    [Fact]
    public async Task MoveOrderUp_Flashcard()
    {
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 1", true, 1), _cancellationToken);

        await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, "Front 1", "Back 1", "Front 1", "Back 1", ["Tag1", "Tag2", "Tag3"], 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, "Front 2", "Back 2", "Front 2", "Back 2", ["Tag1", "Tag2", "Tag3"], 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, "Front 3", "Back 3", "Front 3", "Back 3", ["Tag1", "Tag2", "Tag3"], 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, "Front 4", "Back 4", "Front 4", "Back 4", ["Tag1", "Tag2", "Tag3"], 1), _cancellationToken);

        await _commandDispatcher.Dispatch(new MoveFlashcardOrderUpCommand(1, 4), _cancellationToken);

        var flashcards = await _context.Flashcards
            .OrderBy(c => c.Order)
            .ToListAsync(_cancellationToken);

        flashcards[0].Id.ShouldEqual(1);
        flashcards[1].Id.ShouldEqual(2);
        flashcards[2].Id.ShouldEqual(4);
        flashcards[3].Id.ShouldEqual(3);


        //again
        await _commandDispatcher.Dispatch(new MoveFlashcardOrderUpCommand(1, 4), _cancellationToken);

        flashcards = await _context.Flashcards
            .OrderBy(c => c.Order)
            .ToListAsync(_cancellationToken);

        flashcards[0].Id.ShouldEqual(1);
        flashcards[1].Id.ShouldEqual(4);
        flashcards[2].Id.ShouldEqual(2);
        flashcards[3].Id.ShouldEqual(3);


        //again
        await _commandDispatcher.Dispatch(new MoveFlashcardOrderUpCommand(1, 4), _cancellationToken);

        flashcards = await _context.Flashcards
            .OrderBy(c => c.Order)
            .ToListAsync(_cancellationToken);

        flashcards[0].Id.ShouldEqual(4);
        flashcards[1].Id.ShouldEqual(1);
        flashcards[2].Id.ShouldEqual(2);
        flashcards[3].Id.ShouldEqual(3);

        //again
        await _commandDispatcher.Dispatch(new MoveFlashcardOrderUpCommand(1, 4), _cancellationToken);

        flashcards = await _context.Flashcards
            .OrderBy(c => c.Order)
            .ToListAsync(_cancellationToken);

        flashcards[0].Id.ShouldEqual(4);
        flashcards[1].Id.ShouldEqual(1);
        flashcards[2].Id.ShouldEqual(2);
        flashcards[3].Id.ShouldEqual(3);
    }


    [Fact]
    public async Task Delete_Flashcard()
    {
        await Task.Delay(200,_cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 1", true, 1), _cancellationToken);

        await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, "Front 1", "Back 1", "Front 1", "Back 1", ["Tag1", "Tag2", "Tag3"], 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, "Front 2", "Back 2", "Front 2", "Back 2", ["Tag1", "Tag2", "Tag3"], 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, "Front 3", "Back 3", "Front 3", "Back 3", ["Tag1", "Tag2", "Tag3"], 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddFlashcardCommand(1, "Front 4", "Back 4", "Front 4", "Back 4", ["Tag1", "Tag2", "Tag3"], 1), _cancellationToken);

        _context.Flashcards.Count().ShouldEqual(4);

        await _commandDispatcher.Dispatch(new DeleteFlashcardCommand(2), _cancellationToken);

        _context.Flashcards.Count().ShouldEqual(3);

        var flashcards = await _context.Flashcards
            .OrderBy(c => c.Order)
            .ToListAsync(_cancellationToken);

        flashcards.Count.ShouldEqual(3);

        flashcards[0].Id.ShouldEqual(1);
        flashcards[1].Id.ShouldEqual(3);
        flashcards[1].Order.ShouldEqual((short)2);
        flashcards[2].Id.ShouldEqual(4);
        flashcards[2].Order.ShouldEqual((short)3);
    }
}
