using Autofac;
using CommandHandler_Test.Autofac;
using Dela.Toolkit.Application;
using MyLeitner.Application.Boxes.Queries;
using MyLeitner.Application.Boxes.Queries.Models;
using MyLeitner.Application.Flashcards.Queries;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;
using MyLeitner.Persistence.ReadModel.Sql.ReadModels;
using Should;

namespace CommandHandler_Test;

public class ReadServicesTest : IClassFixture<AutofacTestFixture>
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly ReadModelDbContext _readContext;

    public ReadServicesTest(AutofacTestFixture fixture)
    {
        var scope = fixture.Container.BeginLifetimeScope();
        _queryDispatcher = scope.Resolve<IQueryDispatcher>();
        _readContext = scope.Resolve<ReadModelDbContext>();
    }

    [Fact]
    public async Task GetBoxesQuery_ReturnsBoxWithFlashcardCount()
    {
        var box = new Box { Name = "read box", OrderBox = 1, ReadFlashcardAloud = true, UserId = 1 };
        _readContext.Boxes.Add(box);
        await _readContext.SaveChangesAsync(_cancellationToken);

        _readContext.Flashcards.AddRange(
            new Flashcard { Order = 1, Front = "f1", Back = "b1", Tags = "t", BoxId = box.Id },
            new Flashcard { Order = 2, Front = "f2", Back = "b2", Tags = "t", BoxId = box.Id });
        await _readContext.SaveChangesAsync(_cancellationToken);

        var result = await _queryDispatcher.Dispatch<GetBoxesQuery, List<BoxListItemDto>>(new GetBoxesQuery(1), _cancellationToken);

        var item = result.Single(b => b.Id == box.Id);
        item.Name.ShouldEqual("read box");
        item.TotalFlashcardCount.ShouldEqual(2);
    }

    [Fact]
    public async Task GetBoxQuery_ReturnsNameAndReadAloud()
    {
        var box = new Box { Name = "edit box", OrderBox = 1, ReadFlashcardAloud = true, UserId = 1 };
        _readContext.Boxes.Add(box);
        await _readContext.SaveChangesAsync(_cancellationToken);

        var result = await _queryDispatcher.Dispatch<GetBoxQuery, BoxForEditDto?>(new GetBoxQuery(box.Id), _cancellationToken);

        result.ShouldNotBeNull();
        result!.Name.ShouldEqual("edit box");
        result.ReadFlashcardAloud.ShouldEqual(true);
    }

    [Fact]
    public async Task GetFlashcardsQuery_ReturnsFlashcardsForBox()
    {
        var box = new Box { Name = "fc box", OrderBox = 1, ReadFlashcardAloud = false, UserId = 1 };
        _readContext.Boxes.Add(box);
        await _readContext.SaveChangesAsync(_cancellationToken);

        _readContext.Flashcards.Add(new Flashcard { Order = 1, Front = "front x", Back = "back x", Tags = "a,b", BoxId = box.Id });
        await _readContext.SaveChangesAsync(_cancellationToken);

        var result = await _queryDispatcher.Dispatch<GetFlashcardsQuery, List<FlashcardListItemDto>>(
            new GetFlashcardsQuery(box.Id, 1, 10), _cancellationToken);

        var item = result.Single();
        item.Front.ShouldEqual("front x");
        item.Tags.ShouldContain("a");
        item.Tags.ShouldContain("b");
    }

    [Fact]
    public async Task GetFlashcardQuery_ReturnsFlashcardContent()
    {
        var box = new Box { Name = "fc box 2", OrderBox = 1, ReadFlashcardAloud = false, UserId = 1 };
        _readContext.Boxes.Add(box);
        await _readContext.SaveChangesAsync(_cancellationToken);

        var flashcard = new Flashcard { Order = 1, Front = "front y", Back = "back y", Tags = "tag1", BoxId = box.Id };
        _readContext.Flashcards.Add(flashcard);
        await _readContext.SaveChangesAsync(_cancellationToken);

        var result = await _queryDispatcher.Dispatch<GetFlashcardQuery, FlashcardForSaveDto>(
            new GetFlashcardQuery(flashcard.Id), _cancellationToken);

        result.Front.ShouldEqual("front y");
        result.Back.ShouldEqual("back y");
    }
}
