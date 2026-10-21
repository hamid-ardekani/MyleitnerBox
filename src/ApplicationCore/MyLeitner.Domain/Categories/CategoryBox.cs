using Dela.Toolkit.Domain;

namespace MyLeitner.Domain.Categories;

public class CategoryBox : Entity<long>
{
    public short Order { get; private set; }
    public long CategoryId { get; private set; }
    public long BoxId { get; private set; }

    public static CategoryBox New(short order, long categoryId, long boxId)
    {
        return new CategoryBox { Order = order, CategoryId = categoryId, BoxId = boxId };
    }
}