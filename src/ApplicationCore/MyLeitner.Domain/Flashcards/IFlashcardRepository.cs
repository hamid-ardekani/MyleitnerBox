namespace MyLeitner.Domain.Flashcards;

public interface IFlashcardRepository
{
    Task<Flashcard?> GetByIdAsync(long id, CancellationToken cancellationToken);
    void Remove(Flashcard flashcard);
    Task AddAsync(Flashcard flashcard);
    Task<short> GetMaxOrderByBoxIdAsync(long boxId, CancellationToken cancellationToken);
    Task<Flashcard?> GetByOrderAsync(short order, CancellationToken cancellationToken);
    Task<List<Flashcard>> GetAllByBoxIdAsync(long flashcardBoxId, CancellationToken cancellationToken);
}