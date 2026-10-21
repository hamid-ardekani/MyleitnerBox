using Microsoft.EntityFrameworkCore;
using MyLeitner.Domain.Boxes;

namespace MyLeitner.Persistence.WriteModel.Repositories;

public class BoxRepository : IBoxRepository
{
    private readonly MyLeitnerDbContext _context;

    public BoxRepository(MyLeitnerDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Box box)
    {
        _context.Boxes.AddAsync(box);
        return Task.CompletedTask;
    }

    public void Remove(Box box)
    {
        _context.Boxes.Remove(box);
    }

    public Task<Box?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _context.Boxes 
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public Task<Box?> GetByOrderAsync(short order, CancellationToken cancellationToken)
    {
        return _context.Boxes
            .FirstOrDefaultAsync(b => b.OrderBox == order, cancellationToken);
    }

    public Task<int> GetBoxByUserIdCountAsync(long userId)
    {
        return _context.Boxes.CountAsync(b => b.UserId == userId);
    }

    public Task<List<Box>> GetAllBoxesByUserIdAsync(long userId, CancellationToken cancellationToken)
    {
        return _context.Boxes
            .Where(b=>b.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}