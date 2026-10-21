using Dela.Toolkit.Application;
using MyLeitner.Application.Boxes.Queries.Models;  

namespace MyLeitner.Application.Boxes.Queries;

public record GetBoxesQuery(long UserId) : IQuery;
public class GetBoxesQueryHandler : IQueryHandler<GetBoxesQuery, List<BoxListItemDto>>
{
    private readonly IBoxReadService _service; 

    public GetBoxesQueryHandler(IBoxReadService service )
    {
        _service = service; 
    }

    public Task<List<BoxListItemDto>> Handle(GetBoxesQuery query, CancellationToken cancellationToken)
    {
        return _service.GetAllBoxesForUserAsync(query.UserId); 
    }
}