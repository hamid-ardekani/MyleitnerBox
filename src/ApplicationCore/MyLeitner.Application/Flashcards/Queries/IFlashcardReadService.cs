using MyLeitner.Application.Boxes.Queries.Models; 
using MyLeitner.Application.Flashcards.Queries.Models;

namespace MyLeitner.Application.Flashcards.Queries;

public interface IFlashcardReadService
{
    Task<List<FlashcardListItemDto>> GetFlashcardsByBoxIdAsync(long boxId, int page, int limit, CancellationToken cancellationToken);
    Task<FlashcardForSaveDto> GetFlashcardByIdAsync(long flashcardId, CancellationToken cancellationToken);
    Task<FlashcardSearchResultDto> SearchAsync(SearchFlashcardsQuery query, CancellationToken cancellationToken);
    Task<List<SimilarFlashcardDto>> FindSimilarAsync(FindSimilarFlashcardsQuery query, CancellationToken cancellationToken);
}