using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Boxes.Exceptions;

public class BoxNotFoundException: ExceptionManager
{
    public BoxNotFoundException(long boxId) : base(string.Format(Errors.BoxNotFoundException, boxId))
    {
        
    }
}