using Microsoft.EntityFrameworkCore;
using MyLeitner.Application.Boxes.CommandHandlers;
using MyLeitner.Application.Categories.CommandHandlers;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Domain.Categories;
using MyLeitner.Persistence.WriteModel.Repositories;
using Should;

namespace CommandHandler_Test;

public class CategoryTest
{
    private readonly CancellationToken _cancellationToken;
    private ICategoryRepository _categoryRepository;

    public CategoryTest()
    {
        _cancellationToken = CancellationToken.None;
    }

    [Fact]
    public async Task Add_Category()
    {
        Application.ClearAllTables();

        var context = Application.CreateContext();
        _categoryRepository = new CategoryRepository(context);

        var addBoxCommand = new AddBoxCommandHandler(Application.BoxRepository(context), Application.UnitOfWork(context), Application.UserDomainService);

        await addBoxCommand.Handle(new AddBoxCommand("box cat 1", true, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 2", false, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 3", true, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 4", false, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 5", false, 1), _cancellationToken);


        var command = new AddCategoryCommandHandler(_categoryRepository, Application.UnitOfWork(context), Application.UserDomainService);

        await command.Handle(new AddCategoryCommand("category 1", [1, 2, 3], 1), _cancellationToken);
        await command.Handle(new AddCategoryCommand("category 2", [2, 1, 5, 4], 1), _cancellationToken);
        await command.Handle(new AddCategoryCommand("category 3", [5], 1), _cancellationToken);
        await command.Handle(new AddCategoryCommand("category 4", [1, 5, 3], 1), _cancellationToken);

        context.Categories.Count().ShouldEqual(4);

        var categories = await context.Categories
            .Include(c => c.CategoryBoxes)
            .ToListAsync(_cancellationToken);

        var firstItem = categories[0];
        firstItem.ShouldNotBeNull();
        firstItem.Id.ShouldEqual(1);
        firstItem.Order.ShouldEqual((short)1);
        firstItem.CategoryBoxes.Count.ShouldEqual(3);
        firstItem.CategoryBoxes[0].BoxId.ShouldEqual(1);
        firstItem.CategoryBoxes[0].CategoryId.ShouldEqual(1);
        firstItem.CategoryBoxes[1].BoxId.ShouldEqual(2);
        firstItem.CategoryBoxes[1].CategoryId.ShouldEqual(1);
        firstItem.CategoryBoxes[2].BoxId.ShouldEqual(3);
        firstItem.CategoryBoxes[2].CategoryId.ShouldEqual(1);


        var secondItem = categories[1];
        secondItem.ShouldNotBeNull();
        secondItem.Id.ShouldEqual(2);
        secondItem.Order.ShouldEqual((short)2);
        secondItem.CategoryBoxes.Count.ShouldEqual(4);
        secondItem.CategoryBoxes[0].BoxId.ShouldEqual(2);
        secondItem.CategoryBoxes[0].CategoryId.ShouldEqual(2);
        secondItem.CategoryBoxes[1].BoxId.ShouldEqual(1);
        secondItem.CategoryBoxes[1].CategoryId.ShouldEqual(2);
        secondItem.CategoryBoxes[2].BoxId.ShouldEqual(5);
        secondItem.CategoryBoxes[2].CategoryId.ShouldEqual(2);
        secondItem.CategoryBoxes[3].BoxId.ShouldEqual(4);
        secondItem.CategoryBoxes[3].CategoryId.ShouldEqual(2);


        var thirdItem = categories[2];
        thirdItem.ShouldNotBeNull();
        thirdItem.Id.ShouldEqual(3);
        thirdItem.Order.ShouldEqual((short)3);
        thirdItem.CategoryBoxes.Count.ShouldEqual(1);
        thirdItem.CategoryBoxes[0].BoxId.ShouldEqual(5);
        thirdItem.CategoryBoxes[0].CategoryId.ShouldEqual(3);


        var forthCategoryId = 4;
        var forthItem = categories[3];
        forthItem.ShouldNotBeNull();
        forthItem.Id.ShouldEqual(forthCategoryId);
        forthItem.Order.ShouldEqual((short)4);
        forthItem.CategoryBoxes.Count.ShouldEqual(3);
        forthItem.CategoryBoxes[0].BoxId.ShouldEqual(1);
        forthItem.CategoryBoxes[0].CategoryId.ShouldEqual(forthCategoryId);
        forthItem.CategoryBoxes[1].BoxId.ShouldEqual(5);
        forthItem.CategoryBoxes[1].CategoryId.ShouldEqual(forthCategoryId);
        forthItem.CategoryBoxes[2].BoxId.ShouldEqual(3);
        forthItem.CategoryBoxes[2].CategoryId.ShouldEqual(forthCategoryId);
    }

    [Fact]
    public async Task Update_Category()
    {
        Application.ClearAllTables();

        var context = Application.CreateContext();
        _categoryRepository = new CategoryRepository(context);

        var addBoxCommand = new AddBoxCommandHandler(Application.BoxRepository(context), Application.UnitOfWork(context), Application.UserDomainService);

        await addBoxCommand.Handle(new AddBoxCommand("box cat 1", true, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 2", false, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 3", true, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 4", false, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 5", false, 1), _cancellationToken);


        var addCategoryCommand = new AddCategoryCommandHandler(_categoryRepository, Application.UnitOfWork(context), Application.UserDomainService);
        await addCategoryCommand.Handle(new AddCategoryCommand("category 1", [1, 2, 3], 1), _cancellationToken);

        var updateCategoryCommand = new UpdateCategoryCommandHandler(_categoryRepository, Application.UnitOfWork(context), Application.UserDomainService);
        await updateCategoryCommand.Handle(new UpdateCategoryCommand(1, "category first", [4, 2, 5], 1), _cancellationToken);

        context.Categories.Count().ShouldEqual(1);

        var categories = await context.Categories
            .Include(c => c.CategoryBoxes)
            .ToListAsync(_cancellationToken);

        var firstItem = categories[0];
        firstItem.ShouldNotBeNull();
        firstItem.Id.ShouldEqual(1);
        firstItem.Order.ShouldEqual((short)1);
        firstItem.CategoryBoxes.Count.ShouldEqual(3);
        firstItem.CategoryBoxes[0].BoxId.ShouldEqual(4);
        firstItem.CategoryBoxes[0].CategoryId.ShouldEqual(1);
        firstItem.CategoryBoxes[1].BoxId.ShouldEqual(2);
        firstItem.CategoryBoxes[1].CategoryId.ShouldEqual(1);
        firstItem.CategoryBoxes[2].BoxId.ShouldEqual(5);
        firstItem.CategoryBoxes[2].CategoryId.ShouldEqual(1);
    }

    [Fact]
    public async Task Update_Order_Category_Backward()
    { 
        Application.ClearAllTables();

        var context = Application.CreateContext();
        context.Categories.Count().ShouldEqual(0);

        _categoryRepository = new CategoryRepository(context);
        
        var addBoxCommand = new AddBoxCommandHandler(Application.BoxRepository(context), Application.UnitOfWork(context), Application.UserDomainService);

        await addBoxCommand.Handle(new AddBoxCommand("box cat 1", true, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 2", false, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 3", true, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 4", false, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 5", false, 1), _cancellationToken);


        var command = new AddCategoryCommandHandler(_categoryRepository, Application.UnitOfWork(context), Application.UserDomainService);

        await command.Handle(new AddCategoryCommand("category 1", [1, 2, 3], 1), _cancellationToken);
        await command.Handle(new AddCategoryCommand("category 2", [2, 1, 5, 4], 1), _cancellationToken);
        await command.Handle(new AddCategoryCommand("category 3", [5], 1), _cancellationToken);
        await command.Handle(new AddCategoryCommand("category 4", [1, 5, 3], 1), _cancellationToken);

        var updateCommand = new UpdateOrderCategoryCommandHandler(_categoryRepository, Application.UnitOfWork(context));
        await updateCommand.Handle(new UpdateOrderCategoryCommand(4, 2, 1), _cancellationToken);


        context.Categories.Count().ShouldEqual(4);

        var categories = await context.Categories.ToListAsync(_cancellationToken);

        var firstCategory = categories[0];
        firstCategory.ShouldNotBeNull();
        firstCategory.Id.ShouldEqual(1);
        firstCategory.Order.ShouldEqual((short)1);

        var secondCategory = categories[1];
        secondCategory.ShouldNotBeNull();
        secondCategory.Id.ShouldEqual(2);
        secondCategory.Order.ShouldEqual((short)3);

        var thirdCategory = categories[2];
        thirdCategory.ShouldNotBeNull();
        thirdCategory.Id.ShouldEqual(3);
        thirdCategory.Order.ShouldEqual((short)4);

        var forthCategory = categories[3];
        forthCategory.ShouldNotBeNull();
        forthCategory.Id.ShouldEqual(4);
        forthCategory.Order.ShouldEqual((short)2);
    }

    [Fact]
    public async Task Update_Order_Category_Forward()
    {
        Application.ClearAllTables();

        var context = Application.CreateContext();
        _categoryRepository = new CategoryRepository(context);
        
        var addBoxCommand = new AddBoxCommandHandler(Application.BoxRepository(context), Application.UnitOfWork(context), Application.UserDomainService);

        await addBoxCommand.Handle(new AddBoxCommand("box cat 1", true, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 2", false, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 3", true, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 4", false, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 5", false, 1), _cancellationToken);


        var command = new AddCategoryCommandHandler(_categoryRepository, Application.UnitOfWork(context), Application.UserDomainService);

        await command.Handle(new AddCategoryCommand("category 1", [1, 2, 3], 1), _cancellationToken);
        await command.Handle(new AddCategoryCommand("category 2", [2, 1, 5, 4], 1), _cancellationToken);
        await command.Handle(new AddCategoryCommand("category 3", [5], 1), _cancellationToken);
        await command.Handle(new AddCategoryCommand("category 4", [1, 5, 3], 1), _cancellationToken);

        var updateCommand = new UpdateOrderCategoryCommandHandler(_categoryRepository, Application.UnitOfWork(context));
        await updateCommand.Handle(new UpdateOrderCategoryCommand(2, 4, 1), _cancellationToken);


        context.Categories.Count().ShouldEqual(4);

        var Categories = await context.Categories.ToListAsync(_cancellationToken);

        var firstCategory = Categories[0];
        firstCategory.ShouldNotBeNull();
        firstCategory.Id.ShouldEqual(1);
        firstCategory.Order.ShouldEqual((short)1);

        var secondCategory = Categories[1];
        secondCategory.ShouldNotBeNull();
        secondCategory.Id.ShouldEqual(2);
        secondCategory.Order.ShouldEqual((short)4);

        var thirdCategory = Categories[2];
        thirdCategory.ShouldNotBeNull();
        thirdCategory.Id.ShouldEqual(3);
        thirdCategory.Order.ShouldEqual((short)2);

        var forthCategory = Categories[3];
        forthCategory.ShouldNotBeNull();
        forthCategory.Id.ShouldEqual(4);
        forthCategory.Order.ShouldEqual((short)3);
    }
    
    [Fact]
    public async Task Delete_Category()
    {
        Application.ClearAllTables();
        
        var context = Application.CreateContext();
        _categoryRepository = new CategoryRepository(context);
        
        var addBoxCommand = new AddBoxCommandHandler(Application.BoxRepository(context), Application.UnitOfWork(context), Application.UserDomainService);

        await addBoxCommand.Handle(new AddBoxCommand("box cat 1", true, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 2", false, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 3", true, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 4", false, 1), _cancellationToken);
        await addBoxCommand.Handle(new AddBoxCommand("box cat 5", false, 1), _cancellationToken);


        var command = new AddCategoryCommandHandler(_categoryRepository, Application.UnitOfWork(context), Application.UserDomainService);

        await command.Handle(new AddCategoryCommand("category 1", [1, 2, 3], 1), _cancellationToken);
        await command.Handle(new AddCategoryCommand("category 2", [2, 1, 5, 4], 1), _cancellationToken);
        await command.Handle(new AddCategoryCommand("category 3", [5], 1), _cancellationToken);
        await command.Handle(new AddCategoryCommand("category 4", [1, 5, 3], 1), _cancellationToken);
        
        var updateCommand = new DeleteCategoryCommandHandler(_categoryRepository, new StudySessionRepository(context), Application.UnitOfWork(context));
        await updateCommand.Handle(new DeleteCategoryCommand(2), _cancellationToken);
        
        
        context.Categories.Count().ShouldEqual(3);

        var categories = await context.Categories.ToListAsync(_cancellationToken);
        
        var firstCategory = categories[0];
        firstCategory.ShouldNotBeNull();
        firstCategory.Id.ShouldEqual(1);
        firstCategory.Order.ShouldEqual((short)1);
        
        var thirdCategory = categories[1];
        thirdCategory.ShouldNotBeNull();
        thirdCategory.Id.ShouldEqual(3);
        thirdCategory.Order.ShouldEqual((short)2);
        
        var forthCategory = categories[2];
        forthCategory.ShouldNotBeNull();
        forthCategory.Id.ShouldEqual(4);
        forthCategory.Order.ShouldEqual((short)3);
    }
}