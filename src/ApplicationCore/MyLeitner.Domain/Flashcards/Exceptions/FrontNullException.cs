using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Flashcards.Exceptions;

public class FrontNullException: ExceptionManager
{
    public FrontNullException() : base(string.Format(Errors.FrontNullException))
    {
        
    }
}