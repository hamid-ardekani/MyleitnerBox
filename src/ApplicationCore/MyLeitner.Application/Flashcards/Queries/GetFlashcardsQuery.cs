using Dela.Toolkit.Application; 
using MyLeitner.Application.Boxes.Queries.Models; 

namespace MyLeitner.Application.Flashcards.Queries;

public record GetFlashcardsQuery(long BoxId,int Page, int Limit):IQuery;

public class GetFlashcardsQueryHandler : IQueryHandler<GetFlashcardsQuery, List<FlashcardListItemDto>>
{
    private readonly IFlashcardReadService _service; 

    public GetFlashcardsQueryHandler(IFlashcardReadService service )
    {
        _service = service; 
    }

    public async Task<List<FlashcardListItemDto>> Handle(GetFlashcardsQuery query, CancellationToken cancellationToken)
    {
        return await _service.GetFlashcardsByBoxIdAsync(query.BoxId,query.Page,query.Limit,cancellationToken);
    }
}