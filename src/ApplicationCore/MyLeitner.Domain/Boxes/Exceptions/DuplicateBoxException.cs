using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Boxes.Exceptions;

public class DuplicateBoxException : ExceptionManager
{
    public DuplicateBoxException() : base(Errors.DuplicateBoxException)
    {
    }
}