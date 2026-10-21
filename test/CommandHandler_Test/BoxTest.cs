using Autofac;
using CommandHandler_Test.Autofac;
using Dela.Toolkit.Application;
using Microsoft.EntityFrameworkCore; 
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Domain.Boxes.Exceptions;
using MyLeitner.Persistence.WriteModel;
using Should;

namespace CommandHandler_Test;

public class BoxTest : IClassFixture<AutofacTestFixture>
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly MyLeitnerDbContext _context;
    private readonly ICommandDispatcher _commandDispatcher;

    public BoxTest(AutofacTestFixture fixture)
    {
        var scope = fixture.Container.BeginLifetimeScope();
        _context = scope.Resolve<MyLeitnerDbContext>();

        _commandDispatcher = scope.Resolve<ICommandDispatcher>();
    }

    [Fact]
    public async Task Add_Box()
    {
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 1", true, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 2", false, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 3", true, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 4", false, 1), _cancellationToken);

        _context.Boxes.Count().ShouldEqual(4);
        var lastBox = await _context.Boxes.FirstOrDefaultAsync(c => c.OrderBox == 4, _cancellationToken);

        lastBox.ShouldNotBeNull();

        lastBox!.Name.ShouldEqual("box 4");
        lastBox!.UserId.ShouldEqual(1);
        lastBox!.ReadFlashcardAloud.ShouldEqual(false);
    }

    [Fact]
    public async Task Add_Box_Same_Name_For_User()
    {
        try
        {
            await _commandDispatcher.Dispatch(new AddBoxCommand("box 4", false, 1), _cancellationToken);
            await _commandDispatcher.Dispatch(new AddBoxCommand("box 4", false, 1), _cancellationToken);
        }
        catch (Exception e)
        {
            e.GetType().ShouldEqual(typeof(DuplicateBoxException));
        }
    }

    [Fact]
    public async Task Update_Box()
    {
        var editedName = "box 1 edited";
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 1", false, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new UpdateBoxCommand(1, editedName, true), _cancellationToken);

        _context.Boxes.Count().ShouldEqual(1);
        var lastBox = await _context.Boxes.FirstOrDefaultAsync(_cancellationToken);

        lastBox.ShouldNotBeNull();

        lastBox!.Name.ShouldEqual(editedName);
        lastBox!.UserId.ShouldEqual(1);
        lastBox!.OrderBox.ShouldEqual((short)1);
        lastBox!.ReadFlashcardAloud.ShouldEqual(true);
    }

    [Fact]
    public async Task Update_Order_Box_Backward()
    {
        await Task.Delay(1300, _cancellationToken); 
 
        _context.Boxes.Count().ShouldEqual(0);

        await _commandDispatcher.Dispatch(new AddBoxCommand("box 1", true, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 2", false, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 3", true, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 4", false, 1), _cancellationToken);
        
        await _commandDispatcher.Dispatch(new  UpdateOrderBoxCommand(4, 2, 1), _cancellationToken);
    
        _context.Boxes.Count().ShouldEqual(4);

        var boxes = await _context.Boxes.ToListAsync(_cancellationToken);

        var firstBox = boxes[0];
        firstBox.ShouldNotBeNull();
        firstBox.Id.ShouldEqual(1);
        firstBox.OrderBox.ShouldEqual((short)1);

        var secondBox = boxes[1];
        secondBox.ShouldNotBeNull();
        secondBox.Id.ShouldEqual(2);
        secondBox.OrderBox.ShouldEqual((short)3);

        var thirdBox = boxes[2];
        thirdBox.ShouldNotBeNull();
        thirdBox.Id.ShouldEqual(3);
        thirdBox.OrderBox.ShouldEqual((short)4);

        var forthBox = boxes[3];
        forthBox.ShouldNotBeNull();
        forthBox.Id.ShouldEqual(4);
        forthBox.OrderBox.ShouldEqual((short)2);
    }

    [Fact]
    public async Task Update_Order_Box_Forward()
    { 
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 1", true, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 2", false, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 3", true, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 4", false, 1), _cancellationToken);
        
        await _commandDispatcher.Dispatch(new  UpdateOrderBoxCommand(2, 4, 1), _cancellationToken); 

        _context.Boxes.Count().ShouldEqual(4);

        var boxes = await _context.Boxes.ToListAsync(_cancellationToken);

        var firstBox = boxes[0];
        firstBox.ShouldNotBeNull();
        firstBox.Id.ShouldEqual(1);
        firstBox.OrderBox.ShouldEqual((short)1);

        var secondBox = boxes[1];
        secondBox.ShouldNotBeNull();
        secondBox.Id.ShouldEqual(2);
        secondBox.OrderBox.ShouldEqual((short)4);

        var thirdBox = boxes[2];
        thirdBox.ShouldNotBeNull();
        thirdBox.Id.ShouldEqual(3);
        thirdBox.OrderBox.ShouldEqual((short)2);

        var forthBox = boxes[3];
        forthBox.ShouldNotBeNull();
        forthBox.Id.ShouldEqual(4);
        forthBox.OrderBox.ShouldEqual((short)3);
    }

    [Fact]
    public async Task Delete_Box()
    { 
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 1", true, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 2", false, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 3", true, 1), _cancellationToken);
        await _commandDispatcher.Dispatch(new AddBoxCommand("box 4", false, 1), _cancellationToken);
        
        await _commandDispatcher.Dispatch(new DeleteBoxCommand(2), _cancellationToken); 

        _context.Boxes.Count().ShouldEqual(3);

        var boxes = await _context.Boxes.ToListAsync(_cancellationToken);

        var firstBox = boxes[0];
        firstBox.ShouldNotBeNull();
        firstBox.Id.ShouldEqual(1);
        firstBox.OrderBox.ShouldEqual((short)1);

        var thirdBox = boxes[1];
        thirdBox.ShouldNotBeNull();
        thirdBox.Id.ShouldEqual(3);
        thirdBox.OrderBox.ShouldEqual((short)2);

        var forthBox = boxes[2];
        forthBox.ShouldNotBeNull();
        forthBox.Id.ShouldEqual(4);
        forthBox.OrderBox.ShouldEqual((short)3);
    }
}