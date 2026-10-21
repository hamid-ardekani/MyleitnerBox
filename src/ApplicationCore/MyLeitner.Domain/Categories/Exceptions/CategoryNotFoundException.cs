using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Categories.Exceptions;

public class CategoryNotFoundException: ExceptionManager
{
    public CategoryNotFoundException(long boxId) : base(string.Format(Errors.CategoryNotFoundException, boxId))
    {
        
    }
}