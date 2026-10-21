using Autofac;
using CommandHandler_Test.Autofac;
using Dela.Toolkit.Application;
using MyLeitner.Application.Categories.Queries;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;
using MyLeitner.Persistence.ReadModel.Sql.ReadModels;
using Should;

namespace CommandHandler_Test;

public class GetCategoriesQueryTest : IClassFixture<AutofacTestFixture>
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly ReadModelDbContext _readContext;

    public GetCategoriesQueryTest(AutofacTestFixture fixture)
    {
        var scope = fixture.Container.BeginLifetimeScope();
        _queryDispatcher = scope.Resolve<IQueryDispatcher>();
        _readContext = scope.Resolve<ReadModelDbContext>();
    }

    private async Task<(long CategoryId, long Box1Id, long Box2Id)> SeedCategoryWithTwoBoxesAsync()
    {
        var box1 = new Box { Name = "box 1", OrderBox = 1, ReadFlashcardAloud = false, UserId = 1 };
        var box2 = new Box { Name = "box 2", OrderBox = 2, ReadFlashcardAloud = false, UserId = 1 };
        _readContext.Boxes.AddRange(box1, box2);
        await _readContext.SaveChangesAsync(_cancellationToken);

        var category = new Category { Name = "my category", Order = 1, UserId = 1 };
        _readContext.Categories.Add(category);
        await _readContext.SaveChangesAsync(_cancellationToken);

        _readContext.CategoryBoxes.AddRange(
            new CategoryBox { Order = 1, BoxId = box1.Id, CategoryId = category.Id },
            new CategoryBox { Order = 2, BoxId = box2.Id, CategoryId = category.Id });
        await _readContext.SaveChangesAsync(_cancellationToken);

        return (category.Id, box1.Id, box2.Id);
    }

    [Fact]
    public async Task GetCategoriesQuery_ReturnsCategoryWithBoxCount()
    {
        var (categoryId, _, _) = await SeedCategoryWithTwoBoxesAsync();

        var result = await _queryDispatcher.Dispatch<GetCategoriesQuery, List<CategoryListItemDto>>(new GetCategoriesQuery(1), _cancellationToken);

        var category = result.Single(c => c.Id == categoryId);
        category.Name.ShouldEqual("my category");
        category.BoxCount.ShouldEqual(2);
    }

    [Fact]
    public async Task GetCategoryQuery_ReturnsNameAndOrderedBoxIds()
    {
        var (categoryId, box1Id, box2Id) = await SeedCategoryWithTwoBoxesAsync();

        var result = await _queryDispatcher.Dispatch<GetCategoryQuery, CategoryForEditDto?>(new GetCategoryQuery(categoryId), _cancellationToken);

        result.ShouldNotBeNull();
        result!.Name.ShouldEqual("my category");
        Assert.Equal(new List<long> { box1Id, box2Id }, result.BoxIds);
    }
}
