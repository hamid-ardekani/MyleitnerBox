using System.Collections;

namespace MyLeitner.Domain.Categories;

public interface ICategoryRepository
{
    Task<int> GetCategoryByUserIdCountAsync(long commandUserId);
    Task AddAsync(Category category);
    void Delete(Category category);

    Task<Category?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<Category?> GetByOrderAsync(short order, CancellationToken cancellationToken);
    Task<List<Category>> GetAllCategoriesByUserIdAsync(long userId, CancellationToken cancellationToken); 
}