using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Boxes.Exceptions;

public class StageWeightNullException : ExceptionManager
{
    public StageWeightNullException( ) : base(Errors.StageWeighNullException)
    {
    }
}