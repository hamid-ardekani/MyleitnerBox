using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.Boxes.Exceptions;

public class OrderUnValidException : ExceptionManager
{
    public OrderUnValidException( ) : base(Errors.OrderUnValidException)
    {
    }
}