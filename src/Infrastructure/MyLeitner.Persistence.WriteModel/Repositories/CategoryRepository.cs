 
using Microsoft.EntityFrameworkCore;
using MyLeitner.Domain.Categories; 

namespace MyLeitner.Persistence.WriteModel.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly MyLeitnerDbContext _context;

    public CategoryRepository(MyLeitnerDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Category category)
    {
        _context.Categories.AddAsync(category);
        return Task.CompletedTask;
    }

    public void Delete(Category category)
    {
        _context.Categories.Remove(category);
    }

    public Task<Category?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        // CategoryBoxes باید پیش از Category.Update() (که پاک و دوباره اضافه می‌کند) لود شوند، وگرنه EF ردیف‌های موجود را نمی‌بیند
        // و با هر ذخیره ردیف تازه درج می‌کند؛ نتیجه: تکرار ارتباط یک جعبه در ویرایش‌ها
        return _context.Categories
            .Include(b => b.CategoryBoxes)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public Task<Category?> GetByOrderAsync(short order, CancellationToken cancellationToken)
    {
        return _context.Categories
            .FirstOrDefaultAsync(b => b.Order == order, cancellationToken);
    }

    public Task<int> GetCategoryByUserIdCountAsync(long userId)
    {
        return _context.Categories.CountAsync(b => b.UserId == userId);
    }

    public Task<List<Category>> GetAllCategoriesByUserIdAsync(long userId, CancellationToken cancellationToken)
    {
        return _context.Categories
            .Where(b=>b.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}