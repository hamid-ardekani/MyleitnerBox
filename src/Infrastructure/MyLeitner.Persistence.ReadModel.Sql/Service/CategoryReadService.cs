using Microsoft.EntityFrameworkCore;
using MyLeitner.Application.Categories.Queries;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;

namespace MyLeitner.Persistence.ReadModel.Sql.Service;

public class CategoryReadService(ReadModelDbContext context) : ReadService(context),  ICategoryReadService
{
    public async Task<List<CategoryListItemDto>> GetCategoriesForUserAsync(long userId, CancellationToken cancellationToken)
    {
        return await Context.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Order)
            .Select(c => new CategoryListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Order = c.Order,
                BoxCount = c.CategoryBoxes.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryForEditDto?> GetCategoryForEditAsync(long id, CancellationToken cancellationToken)
    {
        return await Context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryForEditDto
            {
                Name = c.Name,
                BoxIds = c.CategoryBoxes.OrderBy(cb => cb.Order).Select(cb => cb.BoxId).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
