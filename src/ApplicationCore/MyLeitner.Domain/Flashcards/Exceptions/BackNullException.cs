using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Flashcards.Exceptions;

public class BackNullException: ExceptionManager
{
    public BackNullException() : base(string.Format(Errors.BackNullException))
    {
        
    }
}