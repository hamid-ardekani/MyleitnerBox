using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Boxes.Exceptions;

public class FinalLevelUnValidException : ExceptionManager
{
    public FinalLevelUnValidException( ) : base(Errors.FinalLevelUnValidException)
    {
    }
}