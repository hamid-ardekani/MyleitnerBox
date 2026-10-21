using Autofac;
using CommandHandler_Test.Autofac;
using Dela.Toolkit.Application;
using Microsoft.EntityFrameworkCore;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Persistence.WriteModel;
using Should;

namespace CommandHandler_Test;

/// <summary>
/// AddBoxCommandHandler و AddCategoryCommandHandler هر دو UserId واقعی کامند را
/// هاردکد‌شده به ۱ نادیده می‌گرفتند - این تست مطمئن می‌شود که دیگر این‌طور نیست.
/// </summary>
public class UserIdOwnershipTest : IClassFixture<AutofacTestFixture>
{
    private const long RealUserId = 42;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly MyLeitnerDbContext _context;
    private readonly ICommandDispatcher _commandDispatcher;

    public UserIdOwnershipTest(AutofacTestFixture fixture)
    {
        var scope = fixture.Container.BeginLifetimeScope();
        _context = scope.Resolve<MyLeitnerDbContext>();
        _commandDispatcher = scope.Resolve<ICommandDispatcher>();
    }

    [Fact]
    public async Task AddBox_AssignsRealUserId_NotHardcodedOne()
    {
        await _commandDispatcher.Dispatch(new AddBoxCommand("owned box", true, RealUserId), _cancellationToken);

        var box = await _context.Boxes.FirstOrDefaultAsync(b => b.Name == "owned box", _cancellationToken);

        box.ShouldNotBeNull();
        box!.UserId.ShouldEqual(RealUserId);
    }

    [Fact]
    public async Task AddCategory_AssignsRealUserId_NotHardcodedOne()
    {
        await _commandDispatcher.Dispatch(new AddBoxCommand("box for category", true, RealUserId), _cancellationToken);
        var box = await _context.Boxes.FirstOrDefaultAsync(b => b.Name == "box for category", _cancellationToken);

        await _commandDispatcher.Dispatch(new AddCategoryCommand("owned category", [box!.Id], RealUserId), _cancellationToken);

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "owned category", _cancellationToken);

        category.ShouldNotBeNull();
        category!.UserId.ShouldEqual(RealUserId);
    }
}
