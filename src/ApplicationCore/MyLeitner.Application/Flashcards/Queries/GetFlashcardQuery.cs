using Dela.Toolkit.Application;
using MyLeitner.Application.Boxes.Queries.Models;

namespace MyLeitner.Application.Flashcards.Queries;

public record GetFlashcardQuery(long FlashcardId) : IQuery;

public class GetFlashcardQueryHandler : IQueryHandler<GetFlashcardQuery, FlashcardForSaveDto>
{
    private readonly IFlashcardReadService _service;

    public GetFlashcardQueryHandler(IFlashcardReadService service)
    {
        _service = service;
    }

    public async Task<FlashcardForSaveDto> Handle(GetFlashcardQuery query, CancellationToken cancellationToken)
    {
        return await _service.GetFlashcardByIdAsync(query.FlashcardId,cancellationToken);
    }
}