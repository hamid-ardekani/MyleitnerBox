using Microsoft.EntityFrameworkCore;
using MyLeitner.Application.Boxes.Queries;
using MyLeitner.Application.Boxes.Queries.Models;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;

namespace MyLeitner.Persistence.ReadModel.Sql.Service;

public class BoxReadService(ReadModelDbContext context) : ReadService(context), IBoxReadService
{
    public async Task<List<BoxListItemDto>> GetAllBoxesForUserAsync(long userId)
    {
        return await Context.Boxes
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.OrderBox)
            .Select(b => new BoxListItemDto
            {
                Id = b.Id,
                Name = b.Name,
                OrderBox = b.OrderBox,
                StartDate = b.StartDate,
                TotalFlashcardCount = b.Flashcards.Count,
            })
            .ToListAsync();
    }

    public async Task<BoxForEditDto?> GetByIdAsync(long id)
    {
        var box = await Context.Boxes.FirstOrDefaultAsync(c => c.Id == id);

        return box != null ? new BoxForEditDto(box.Name, box.ReadFlashcardAloud, box.DailyNewCardLimit, box.DailyBacklogFloor) : null;
    }
}
