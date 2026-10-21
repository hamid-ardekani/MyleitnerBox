using Dela.Toolkit.Domain;
using MyLeitner.Domain.Boxes.Exceptions;

namespace MyLeitner.Domain.Categories;

public class Category : AggregateRoot<long>
{
    public string Name { get; private set; } = null!;
    public short Order { get; private set; }
    public long UserId { get; private set; }
    public List<CategoryBox> CategoryBoxes { get; private set; } = [];

    public static Category CategoryForAdd(string name, short order, long userId)
    {
        return new Category
        {
            Name = name,
            Order = order,
            UserId = userId,
        };
    }

    public void AddBox(long boxId)
    {
        var order = CategoryBoxes.Count > 0
            ? CategoryBoxes.Max(x => x.Order) + 1
            : 1;

        CategoryBoxes.Add(CategoryBox.New((short)order, Id, boxId));
    }

    public void Update(string name, List<long> boxIds)
    {
        Name = name;

        CategoryBoxes.Clear();

        var order = 1;
        foreach (var boxId in boxIds)
        {
            CategoryBoxes.Add(CategoryBox.New((short)order, Id, boxId));
            order++;
        }
    }

    public void UpdateOrder(short order)
    {
        if (order <= 0)
            throw new OrderUnValidException();

        Order = order;
    }
}