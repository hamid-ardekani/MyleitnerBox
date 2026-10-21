namespace MyLeitner.Application.Categories.Queries;

public class CategoryListItemDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public short Order { get; set; }
    public int BoxCount { get; set; }
}

public class CategoryForEditDto
{
    public string Name { get; set; } = null!;
    public List<long> BoxIds { get; set; } = [];
}
