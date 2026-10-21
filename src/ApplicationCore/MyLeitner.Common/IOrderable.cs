namespace MyLeitner.Common;

public interface IOrderable
{
    long Id { get; }
    short Order { get; }
    void UpdateOrder(short order); 
}

public class OrderItem : IOrderable
{
    public OrderItem(long id, short order)
    {
        Id = id;
        Order = order;
    }

    public long Id { get; init; }
    public short Order { get; private set; }
    public void UpdateOrder(short order)
    {
        Order = order;
    }
}