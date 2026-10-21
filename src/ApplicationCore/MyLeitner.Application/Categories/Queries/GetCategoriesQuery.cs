using Dela.Toolkit.Application;

namespace MyLeitner.Application.Categories.Queries;

public record GetCategoriesQuery(long UserId) : IQuery;

public class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, List<CategoryListItemDto>>
{
    private readonly ICategoryReadService _service;

    public GetCategoriesQueryHandler(ICategoryReadService service)
    {
        _service = service;
    }

    public Task<List<CategoryListItemDto>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        return _service.GetCategoriesForUserAsync(query.UserId, cancellationToken);
    }
}

public record GetCategoryQuery(long Id) : IQuery;

public class GetCategoryQueryHandler : IQueryHandler<GetCategoryQuery, CategoryForEditDto?>
{
    private readonly ICategoryReadService _service;

    public GetCategoryQueryHandler(ICategoryReadService service)
    {
        _service = service;
    }

    public Task<CategoryForEditDto?> Handle(GetCategoryQuery query, CancellationToken cancellationToken)
    {
        return _service.GetCategoryForEditAsync(query.Id, cancellationToken);
    }
}
