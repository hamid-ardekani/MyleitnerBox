using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Flashcards.Exceptions;

public class TagsIsEmptyException: ExceptionManager
{
    public TagsIsEmptyException() : base(string.Format(Errors.TagsIsEmptyException))
    {
        
    }
}