using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Boxes.Exceptions;

public class DailyNewFlashcardCountUnValidException : ExceptionManager
{
    public DailyNewFlashcardCountUnValidException( ) : base(Errors.DailyNewFlashcardCountUnValidException)
    {
    }
}