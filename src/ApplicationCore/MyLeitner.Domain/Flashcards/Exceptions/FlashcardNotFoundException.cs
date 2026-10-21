using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Flashcards.Exceptions;

public class FlashcardNotFoundException: ExceptionManager
{
    public FlashcardNotFoundException(long flashcardId) : base(string.Format(Errors.FlashcardNotFoundException, flashcardId))
    {
        
    }
}