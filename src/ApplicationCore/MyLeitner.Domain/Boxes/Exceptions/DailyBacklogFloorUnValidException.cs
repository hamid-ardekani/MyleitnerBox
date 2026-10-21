using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Boxes.Exceptions;

public class DailyBacklogFloorUnValidException : ExceptionManager
{
    public DailyBacklogFloorUnValidException() : base(Errors.DailyBacklogFloorUnValidException)
    {
    }
}
