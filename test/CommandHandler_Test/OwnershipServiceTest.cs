using Autofac;
using CommandHandler_Test.Autofac;
using MyLeitner.Application.Contract.Services;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;
using MyLeitner.Persistence.ReadModel.Sql.ReadModels;
using Should;

namespace CommandHandler_Test;

/// <summary>چک مالکیت روی داده‌ی واقعی (دیتابیس in-memory): فقط مالکِ جعبه/دسته/کارت «مالک» حساب می‌شود.</summary>
public class OwnershipServiceTest : IClassFixture<AutofacTestFixture>
{
    private static long _nextUser = 5000;

    private readonly CancellationToken _ct = CancellationToken.None;
    private readonly ReadModelDbContext _context;
    private readonly IOwnershipService _ownership;

    private readonly long _owner = Interlocked.Add(ref _nextUser, 10);
    private readonly long _other;

    public OwnershipServiceTest(AutofacTestFixture fixture)
    {
        _other = _owner + 1;
        var scope = fixture.Container.BeginLifetimeScope();
        _context = scope.Resolve<ReadModelDbContext>();
        _ownership = scope.Resolve<IOwnershipService>();
    }

    private async Task<Box> AddBoxAsync(long userId, string name)
    {
        var box = new Box { Name = name, OrderBox = 1, ReadFlashcardAloud = false, UserId = userId };
        _context.Boxes.Add(box);
        await _context.SaveChangesAsync(_ct);
        return box;
    }

    [Fact]
    public async Task Box_OnlyTheOwnerOwnsIt()
    {
        var box = await AddBoxAsync(_owner, "own-box");

        (await _ownership.OwnsBoxAsync(_owner, box.Id, _ct)).ShouldBeTrue();
        (await _ownership.OwnsBoxAsync(_other, box.Id, _ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task Box_NonexistentIsNotOwned()
    {
        (await _ownership.OwnsBoxAsync(_owner, 987_654_321, _ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task Boxes_AllMustBeOwned()
    {
        var mine1 = await AddBoxAsync(_owner, "mine-1");
        var mine2 = await AddBoxAsync(_owner, "mine-2");
        var theirs = await AddBoxAsync(_other, "theirs");

        (await _ownership.OwnsBoxesAsync(_owner, [mine1.Id, mine2.Id], _ct)).ShouldBeTrue();
        (await _ownership.OwnsBoxesAsync(_owner, [mine1.Id, theirs.Id], _ct)).ShouldBeFalse(); // یک جعبهٔ غریبه کافی است
        (await _ownership.OwnsBoxesAsync(_owner, [mine1.Id, 987_654_321], _ct)).ShouldBeFalse(); // ناموجود هم مالک نیست
    }

    [Fact]
    public async Task Boxes_DuplicateIdsDoNotBreakTheCheck()
    {
        var mine = await AddBoxAsync(_owner, "dup");

        (await _ownership.OwnsBoxesAsync(_owner, [mine.Id, mine.Id, mine.Id], _ct)).ShouldBeTrue();
    }

    [Fact]
    public async Task Boxes_EmptyListIsFine()
    {
        (await _ownership.OwnsBoxesAsync(_owner, [], _ct)).ShouldBeTrue();
    }

    [Fact]
    public async Task Category_OnlyTheOwnerOwnsIt()
    {
        var category = new Category { Name = "own-cat", Order = 1, UserId = _owner };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(_ct);

        (await _ownership.OwnsCategoryAsync(_owner, category.Id, _ct)).ShouldBeTrue();
        (await _ownership.OwnsCategoryAsync(_other, category.Id, _ct)).ShouldBeFalse();
        (await _ownership.OwnsCategoryAsync(_owner, 987_654_321, _ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task Flashcard_OwnershipFollowsItsBox()
    {
        var box = await AddBoxAsync(_owner, "card-box");
        var card = new Flashcard { Order = 1, Front = "f", Back = "b", Tags = "", BoxId = box.Id };
        _context.Flashcards.Add(card);
        await _context.SaveChangesAsync(_ct);

        (await _ownership.OwnsFlashcardAsync(_owner, card.Id, _ct)).ShouldBeTrue();
        (await _ownership.OwnsFlashcardAsync(_other, card.Id, _ct)).ShouldBeFalse();
        (await _ownership.OwnsFlashcardAsync(_owner, 987_654_321, _ct)).ShouldBeFalse();
    }
}
