using System.Security.Claims;
using Dela.Toolkit.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MyLeitner.Api.Controllers;
using MyLeitner.Application.Contract.Services;
using MyLeitner.Common.Results;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;
using MyLeitner.Persistence.ReadModel.Sql.ReadModels;

namespace CommandHandler_Test;

/// <summary>Dispatcher جعلی: فقط می‌شمارد چه چیزی به لایهٔ برنامه رسیده و پاسخ پیش‌فرض می‌دهد.</summary>
internal sealed class RecordingQueryDispatcher : IQueryDispatcher
{
    public List<object> Queries { get; } = [];

    public Task<TQueryResult> Dispatch<TQuery, TQueryResult>(TQuery query, CancellationToken cancellationToken) where TQuery : IQuery
    {
        Queries.Add(query!);
        return Task.FromResult(DefaultFor<TQueryResult>());
    }

    // کنترلر Next برای دسته، Result<T> می‌خواهد (IsFailure روی null می‌ترکد)
    private static T DefaultFor<T>()
    {
        var type = typeof(T);
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = type.GetGenericArguments()[0];
            var value = valueType.IsValueType ? Activator.CreateInstance(valueType) : null;
            return (T)type.GetMethod(nameof(Result<object>.Success))!.Invoke(null, [value])!;
        }

        return default!;
    }
}

internal sealed class RecordingCommandDispatcher : ICommandDispatcher
{
    public List<object> Commands { get; } = [];

    public Task Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : ICommand
    {
        Commands.Add(command!);
        return Task.CompletedTask;
    }

    public Task<TCommandResult> DispatchAsync<TCommand, TCommandResult>(TCommand command, CancellationToken cancellationToken) where TCommand : ICommand
    {
        Commands.Add(command!);
        return Task.FromResult(default(TCommandResult)!);
    }
}

/// <summary>یک کاربر (با توکن فرضی) که سه کنترلرِ محافظت‌شده را صدا می‌زند.</summary>
internal sealed class Actor
{
    public required BoxController Box { get; init; }
    public required CategoryController Category { get; init; }
    public required LearningController Learning { get; init; }
    public required RecordingQueryDispatcher Queries { get; init; }
    public required RecordingCommandDispatcher Commands { get; init; }
    public required GuardWorld World { get; init; }

    public int Dispatches => Queries.Queries.Count + Commands.Commands.Count;
}

/// <summary>
/// دو کاربر با داده‌ی مخصوص خودشان در دیتابیس in-memory: A (مالک) و B (غریبه). شناسه‌ها هر بار تازه‌اند تا تست‌ها با هم تداخل نکنند.
/// </summary>
internal sealed class GuardWorld
{
    private static long _nextUser = 20_000;

    private readonly IOwnershipService _ownership;

    public long OwnerId { get; }
    public long StrangerId { get; }
    public long SuperId { get; }

    public long BoxA { get; private set; }
    public long BoxB { get; private set; }
    public long CategoryA { get; private set; }
    public long CategoryB { get; private set; }
    public long CardA { get; private set; }
    public long CardB { get; private set; }

    public const long Missing = 987_654_321;

    public GuardWorld(IOwnershipService ownership)
    {
        _ownership = ownership;
        OwnerId = Interlocked.Add(ref _nextUser, 10);
        StrangerId = OwnerId + 1;
        SuperId = OwnerId + 2;
    }

    public static async Task<GuardWorld> CreateAsync(ReadModelDbContext context, IOwnershipService ownership)
    {
        var world = new GuardWorld(ownership);

        var boxA = new Box { Name = "A", OrderBox = 1, ReadFlashcardAloud = false, UserId = world.OwnerId };
        var boxB = new Box { Name = "B", OrderBox = 1, ReadFlashcardAloud = false, UserId = world.StrangerId };
        context.Boxes.AddRange(boxA, boxB);
        await context.SaveChangesAsync();

        var catA = new Category { Name = "A", Order = 1, UserId = world.OwnerId };
        var catB = new Category { Name = "B", Order = 1, UserId = world.StrangerId };
        context.Categories.AddRange(catA, catB);

        var cardA = new Flashcard { Order = 1, Front = "f", Back = "b", Tags = "", BoxId = boxA.Id };
        var cardB = new Flashcard { Order = 1, Front = "f", Back = "b", Tags = "", BoxId = boxB.Id };
        context.Flashcards.AddRange(cardA, cardB);
        await context.SaveChangesAsync();

        world.BoxA = boxA.Id;
        world.BoxB = boxB.Id;
        world.CategoryA = catA.Id;
        world.CategoryB = catB.Id;
        world.CardA = cardA.Id;
        world.CardB = cardB.Id;
        return world;
    }

    public Actor ActAs(long userId, bool superUser = false)
    {
        var queries = new RecordingQueryDispatcher();
        var commands = new RecordingCommandDispatcher();

        var claims = new List<Claim> { new("sub", userId.ToString()) };
        if (superUser)
            claims.Add(new Claim("is_super", "true"));

        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")),
            RequestServices = new ServiceCollection().AddSingleton(_ownership).BuildServiceProvider(),
        };

        T Make<T>() where T : ControllerBase
        {
            var controller = (T)Activator.CreateInstance(typeof(T), queries, commands)!;
            controller.ControllerContext = new ControllerContext { HttpContext = http };
            return controller;
        }

        return new Actor
        {
            Box = Make<BoxController>(),
            Category = Make<CategoryController>(),
            Learning = Make<LearningController>(),
            Queries = queries,
            Commands = commands,
            World = this,
        };
    }
}
