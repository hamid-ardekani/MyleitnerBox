namespace MyLeitner.Application.Categories.Queries;

public interface ICategoryReadService
{
    Task<List<CategoryListItemDto>> GetCategoriesForUserAsync(long userId, CancellationToken cancellationToken);
    Task<CategoryForEditDto?> GetCategoryForEditAsync(long id, CancellationToken cancellationToken);
}