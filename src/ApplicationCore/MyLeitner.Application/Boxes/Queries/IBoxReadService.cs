using MyLeitner.Application.Boxes.Queries.Models;

namespace MyLeitner.Application.Boxes.Queries;

public interface IBoxReadService
{
    public Task<List<BoxListItemDto>> GetAllBoxesForUserAsync(long userId);
    public Task<BoxForEditDto?> GetByIdAsync(long id);
}