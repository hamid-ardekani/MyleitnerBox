using System.Reflection;
using Autofac;
using CommandHandler_Test.Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyLeitner.Api.Controllers;
using MyLeitner.Api.Models;
using MyLeitner.Application.Boxes.Queries.Models;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Application.Contract.Services;
using MyLeitner.Application.Learnings.Commands;
using MyLeitner.Application.Learnings.Enums;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;
using Should;

namespace CommandHandler_Test;

/// <summary>
/// گاردهای مالکیت کنترلرها: کاربر غریبه به داده‌ی دیگری دست نمی‌زند (NotFound و هیچ چیزی به لایهٔ برنامه نمی‌رسد)،
/// مالک و کاربر ویژه آزادند، و UserId همیشه از توکن می‌آید نه از بدنهٔ درخواست.
/// </summary>
public class ControllerOwnershipGuardTest : IClassFixture<AutofacTestFixture>
{
    private readonly CancellationToken _ct = CancellationToken.None;
    private readonly AutofacTestFixture _fixture;

    public ControllerOwnershipGuardTest(AutofacTestFixture fixture) => _fixture = fixture;

    private async Task<GuardWorld> NewWorldAsync()
    {
        var scope = _fixture.Container.BeginLifetimeScope();
        return await GuardWorld.CreateAsync(scope.Resolve<ReadModelDbContext>(), scope.Resolve<IOwnershipService>());
    }

    private static readonly FlashcardForSaveDto SomeCard = new(0, "f", "b", "f", "b", []);

    // هر endpointی که با شناسه‌ی منبع A کار می‌کند؛ همه باید برای غیرِ مالک NotFound بدهند.
    private static readonly Dictionary<string, Func<Actor, Task<ActionResult>>> OwnedEndpoints = new()
    {
        ["Box.Get"] = a => a.Box.GetBoxAsync(a.World.BoxA, default),
        ["Box.Update"] = a => a.Box.UpdateBoxAsync(new UpdateBoxCommand(a.World.BoxA, "x", false), default),
        ["Box.UpdateOrder"] = a => a.Box.UpdateBoxAsync(new UpdateOrderBoxCommand(a.World.BoxA, 1, 0), default),
        ["Box.Delete"] = a => a.Box.DeleteBoxAsync(a.World.BoxA, default),
        ["Box.GetFlashcards"] = a => a.Box.GetFlashcardsAsync(a.World.BoxA, 1, 10, default),
        ["Box.GetFlashcard"] = a => a.Box.GetFlashcardAsync(a.World.CardA, default),
        ["Box.AddFlashcard"] = a => a.Box.AddFlashcardAsync(a.World.BoxA, SomeCard, default),
        ["Box.UpdateFlashcard"] = a => a.Box.UpdateFlashcardAsync(a.World.BoxA, SomeCard with { FlashcardId = a.World.CardA }, default),
        ["Box.DeleteFlashcard"] = a => a.Box.DeleteFlashcardAsync(a.World.BoxA, a.World.CardA, default),
        ["Box.ArchiveFlashcard"] = a => a.Box.ArchiveFlashcardAsync(a.World.BoxA, a.World.CardA, default),
        ["Box.RestoreFlashcard"] = a => a.Box.RestoreFlashcardAsync(a.World.BoxA, a.World.CardA, default),
        ["Box.AddToReview"] = a => a.Box.AddFlashcardToReviewAsync(a.World.BoxA, a.World.CardA, default),

        ["Category.Get"] = a => a.Category.GetCategoryAsync(a.World.CategoryA, default),
        ["Category.Add"] = a => a.Category.AddCategoryAsync(new AddCategoryCommand("x", [a.World.BoxA], 0), default),
        ["Category.Update"] = a => a.Category.UpdateCategoryAsync(new UpdateCategoryCommand(a.World.CategoryA, "x", [a.World.BoxA], 0), default),
        ["Category.Delete"] = a => a.Category.DeleteCategoryAsync(a.World.CategoryA, default),

        ["Learning.NextInCategory"] = a => a.Learning.GetNextPlayFlashcardInCategoryProcess(a.World.CategoryA, default),
        ["Learning.NextInBox"] = a => a.Learning.GetNextPlayFlashcardInBoxProcess(a.World.BoxA, default),
        ["Learning.OverviewBox"] = a => a.Learning.GetStudyOverview(a.World.BoxA, null, default),
        ["Learning.OverviewCategory"] = a => a.Learning.GetStudyOverview(null, a.World.CategoryA, default),
        ["Learning.PracticeBox"] = a => a.Learning.GetPractice(a.World.BoxA, null, default),
        ["Learning.PracticeCategory"] = a => a.Learning.GetPractice(null, a.World.CategoryA, default),
        ["Learning.Preview"] = a => a.Learning.GetAnswerPreview(a.World.CardA, 3, default),
        ["Learning.AnswerProcess"] = a => a.Learning.AnswerProcess(new AnswerProcessRequest(a.World.CardA, AnswerRateTimeType.Confident, 3, null), default),
        ["Learning.ShowLater"] = a => a.Learning.ShowLaterProcess(new ShowLaterProcessRequest(a.World.CardA, null), default),
        ["Learning.MarkKnown"] = a => a.Learning.MarkKnown(new MarkKnownRequest(a.World.CardA, null), default),
    };

    // مالک A است ولی یکی از شناسه‌های ورودی مال B است: این هم باید رد شود (حدس زدن/تزریق شناسه‌ی غریبه).
    private static readonly Dictionary<string, Func<Actor, Task<ActionResult>>> ForeignReferences = new()
    {
        ["Box.UpdateFlashcard: move to someone else's box"] = a => a.Box.UpdateFlashcardAsync(a.World.BoxA, SomeCard with { FlashcardId = a.World.CardA, MoveToBoxId = a.World.BoxB }, default),
        ["Box.UpdateFlashcard: someone else's card"] = a => a.Box.UpdateFlashcardAsync(a.World.BoxA, SomeCard with { FlashcardId = a.World.CardB }, default),
        ["Box.GetFlashcard: someone else's card"] = a => a.Box.GetFlashcardAsync(a.World.CardB, default),
        ["Category.Add: someone else's box"] = a => a.Category.AddCategoryAsync(new AddCategoryCommand("x", [a.World.BoxA, a.World.BoxB], 0), default),
        ["Category.Update: someone else's box"] = a => a.Category.UpdateCategoryAsync(new UpdateCategoryCommand(a.World.CategoryA, "x", [a.World.BoxB], 0), default),
        ["Category.Update: someone else's category"] = a => a.Category.UpdateCategoryAsync(new UpdateCategoryCommand(a.World.CategoryB, "x", [a.World.BoxA], 0), default),
        ["Learning.Overview: someone else's category"] = a => a.Learning.GetStudyOverview(null, a.World.CategoryB, default),
        ["Learning.AnswerProcess: someone else's category"] = a => a.Learning.AnswerProcess(new AnswerProcessRequest(a.World.CardA, AnswerRateTimeType.Confident, 3, a.World.CategoryB), default),
        ["Learning.ShowLater: someone else's category"] = a => a.Learning.ShowLaterProcess(new ShowLaterProcessRequest(a.World.CardA, a.World.CategoryB), default),
        ["Learning.MarkKnown: someone else's category"] = a => a.Learning.MarkKnown(new MarkKnownRequest(a.World.CardA, a.World.CategoryB), default),
    };

    private static readonly Dictionary<string, Func<Actor, Task<ActionResult>>> MissingResources = new()
    {
        ["Box.Get"] = a => a.Box.GetBoxAsync(GuardWorld.Missing, default),
        ["Box.Delete"] = a => a.Box.DeleteBoxAsync(GuardWorld.Missing, default),
        ["Box.GetFlashcard"] = a => a.Box.GetFlashcardAsync(GuardWorld.Missing, default),
        ["Category.Get"] = a => a.Category.GetCategoryAsync(GuardWorld.Missing, default),
        ["Learning.NextInBox"] = a => a.Learning.GetNextPlayFlashcardInBoxProcess(GuardWorld.Missing, default),
        ["Learning.Preview"] = a => a.Learning.GetAnswerPreview(GuardWorld.Missing, 3, default),
    };

    public static TheoryData<string> OwnedNames => [.. OwnedEndpoints.Keys];
    public static TheoryData<string> ForeignNames => [.. ForeignReferences.Keys];
    public static TheoryData<string> MissingNames => [.. MissingResources.Keys];

    [Theory]
    [MemberData(nameof(OwnedNames))]
    public async Task Stranger_GetsNotFound_AndNothingReachesTheApplication(string endpoint)
    {
        var world = await NewWorldAsync();
        var stranger = world.ActAs(world.StrangerId);

        var result = await OwnedEndpoints[endpoint](stranger);

        result.ShouldBeType<NotFoundResult>();
        stranger.Dispatches.ShouldEqual(0);
    }

    [Theory]
    [MemberData(nameof(OwnedNames))]
    public async Task Owner_IsLetThrough(string endpoint)
    {
        var world = await NewWorldAsync();
        var owner = world.ActAs(world.OwnerId);

        var result = await OwnedEndpoints[endpoint](owner);

        result.ShouldNotBeType<NotFoundResult>();
        owner.Dispatches.ShouldBeGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(OwnedNames))]
    public async Task SuperUser_HasAccessToEverything(string endpoint)
    {
        var world = await NewWorldAsync();
        var superUser = world.ActAs(world.SuperId, superUser: true);

        var result = await OwnedEndpoints[endpoint](superUser);

        result.ShouldNotBeType<NotFoundResult>();
        superUser.Dispatches.ShouldBeGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(ForeignNames))]
    public async Task Owner_CannotReferenceSomeoneElsesResources(string endpoint)
    {
        var world = await NewWorldAsync();
        var owner = world.ActAs(world.OwnerId);

        var result = await ForeignReferences[endpoint](owner);

        result.ShouldBeType<NotFoundResult>();
        owner.Dispatches.ShouldEqual(0);
    }

    [Theory]
    [MemberData(nameof(MissingNames))]
    public async Task NonexistentResource_IsNotFound_ForEveryone(string endpoint)
    {
        var world = await NewWorldAsync();
        var owner = world.ActAs(world.OwnerId);

        var result = await MissingResources[endpoint](owner);

        result.ShouldBeType<NotFoundResult>();
        owner.Dispatches.ShouldEqual(0);
    }

    // ---- UserId از توکن می‌آید، نه از بدنهٔ درخواست (کلاینت نمی‌تواند به نام دیگری کار کند) ----

    [Fact]
    public async Task AddBox_UsesTheTokenUser_NotTheOneInTheBody()
    {
        var world = await NewWorldAsync();
        var owner = world.ActAs(world.OwnerId);

        await owner.Box.AddBoxAsync(new AddBoxCommand("x", false, world.StrangerId), _ct);

        owner.Commands.Commands.OfType<AddBoxCommand>().Single().UserId.ShouldEqual(world.OwnerId);
    }

    [Fact]
    public async Task UpdateOrderBox_UsesTheTokenUser()
    {
        var world = await NewWorldAsync();
        var owner = world.ActAs(world.OwnerId);

        await owner.Box.UpdateBoxAsync(new UpdateOrderBoxCommand(world.BoxA, 2, world.StrangerId), _ct);

        owner.Commands.Commands.OfType<UpdateOrderBoxCommand>().Single().UserId.ShouldEqual(world.OwnerId);
    }

    [Fact]
    public async Task AddAndUpdateCategory_UseTheTokenUser()
    {
        var world = await NewWorldAsync();
        var owner = world.ActAs(world.OwnerId);

        await owner.Category.AddCategoryAsync(new AddCategoryCommand("x", [world.BoxA], world.StrangerId), _ct);
        await owner.Category.UpdateCategoryAsync(new UpdateCategoryCommand(world.CategoryA, "y", [world.BoxA], world.StrangerId), _ct);

        owner.Commands.Commands.OfType<AddCategoryCommand>().Single().UserId.ShouldEqual(world.OwnerId);
        owner.Commands.Commands.OfType<UpdateCategoryCommand>().Single().UserId.ShouldEqual(world.OwnerId);
    }

    [Fact]
    public async Task AnswerProcess_UsesTheTokenUser()
    {
        var world = await NewWorldAsync();
        var owner = world.ActAs(world.OwnerId);

        await owner.Learning.AnswerProcess(new AnswerProcessRequest(world.CardA, AnswerRateTimeType.Confident, 3, null), _ct);

        owner.Commands.Commands.OfType<AnswerProcessCommand>().Single().UserId.ShouldEqual(world.OwnerId);
    }

    // ---- هر کنترلر جز Auth باید پشت [Authorize] باشد و هیچ action ای AllowAnonymous نباشد ----

    private static IEnumerable<Type> ApiControllers() =>
        typeof(BoxController).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

    [Fact]
    public void EveryController_ExceptAuth_RequiresAuthorization()
    {
        var unprotected = ApiControllers()
            .Where(t => t != typeof(AuthController))
            .Where(t => !t.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any())
            .Select(t => t.Name)
            .ToList();

        unprotected.ShouldBeEmpty();
    }

    [Fact]
    public void ProtectedControllers_HaveNoAnonymousActions()
    {
        var anonymous = ApiControllers()
            .Where(t => t != typeof(AuthController))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true).Any())
                .Select(m => $"{t.Name}.{m.Name}"))
            .ToList();

        anonymous.ShouldBeEmpty();
    }

    [Fact]
    public void AuthController_IsTheOnlyAnonymousController()
    {
        typeof(AuthController).GetCustomAttributes<AllowAnonymousAttribute>(inherit: true).Any().ShouldBeTrue();
    }
}
