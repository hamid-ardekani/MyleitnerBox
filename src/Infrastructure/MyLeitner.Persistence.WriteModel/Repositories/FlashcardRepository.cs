using Microsoft.EntityFrameworkCore;
using MyLeitner.Domain.Flashcards;

namespace MyLeitner.Persistence.WriteModel.Repositories;

public class FlashcardRepository : IFlashcardRepository
{
    private readonly MyLeitnerDbContext _context;

    public FlashcardRepository(MyLeitnerDbContext context)
    {
        _context = context;
    }

    public Task<Flashcard?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _context.Flashcards
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public void Remove(Flashcard flashcard)
    {
        _context.Remove(flashcard);
    }

    public async Task AddAsync(Flashcard flashcard)
    {
        await _context.Flashcards.AddAsync(flashcard);
    }

    public async Task<short> GetMaxOrderByBoxIdAsync(long boxId, CancellationToken cancellationToken)
    {
        var hasAny = await _context.Flashcards.AnyAsync(f => f.BoxId == boxId, cancellationToken);
        if (!hasAny)
            return 0;

        return await _context.Flashcards
            .Where(f => f.BoxId == boxId)
            .MaxAsync(f => f.Order, cancellationToken);
    }

    public Task<Flashcard?> GetByOrderAsync(short order, CancellationToken cancellationToken)
    {
        return _context.Flashcards 
            .FirstOrDefaultAsync(f => f.Order == order, cancellationToken);
    }

    public Task<List<Flashcard>> GetAllByBoxIdAsync(long boxId, CancellationToken cancellationToken)
    {
        return _context.Flashcards
            .Where(f => f.BoxId == boxId)
            .ToListAsync(cancellationToken);
    }
}