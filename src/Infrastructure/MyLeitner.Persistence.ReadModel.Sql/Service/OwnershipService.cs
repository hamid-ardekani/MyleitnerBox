using Microsoft.EntityFrameworkCore;
using MyLeitner.Application.Contract.Services;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;

namespace MyLeitner.Persistence.ReadModel.Sql.Service;

public class OwnershipService(ReadModelDbContext context) : IOwnershipService
{
    public Task<bool> OwnsBoxAsync(long userId, long boxId, CancellationToken ct) =>
        context.Boxes.AnyAsync(b => b.Id == boxId && b.UserId == userId, ct);

    public async Task<bool> OwnsBoxesAsync(long userId, IReadOnlyCollection<long> boxIds, CancellationToken ct)
    {
        var distinct = boxIds.Distinct().ToList();
        if (distinct.Count == 0)
            return true;

        var owned = await context.Boxes.CountAsync(b => b.UserId == userId && distinct.Contains(b.Id), ct);
        return owned == distinct.Count;
    }

    public Task<bool> OwnsCategoryAsync(long userId, long categoryId, CancellationToken ct) =>
        context.Categories.AnyAsync(c => c.Id == categoryId && c.UserId == userId, ct);

    public Task<bool> OwnsFlashcardAsync(long userId, long flashcardId, CancellationToken ct) =>
        context.Flashcards.AnyAsync(f => f.Id == flashcardId && f.Box.UserId == userId, ct);
}
