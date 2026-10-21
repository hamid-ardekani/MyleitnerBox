using Dela.Toolkit.Application;
using MyLeitner.Application.Boxes.Queries.Models;  

namespace MyLeitner.Application.Boxes.Queries;

public record GetBoxQuery(long Id) : IQuery;
public class GetBoxQueryHandler : IQueryHandler<GetBoxQuery, BoxForEditDto?>
{
    private readonly IBoxReadService _service; 

    public GetBoxQueryHandler(IBoxReadService service )
    {
        _service = service; 
    }

    public Task<BoxForEditDto?> Handle(GetBoxQuery query, CancellationToken cancellationToken)
    {
        return _service.GetByIdAsync(query.Id); 
    }
}