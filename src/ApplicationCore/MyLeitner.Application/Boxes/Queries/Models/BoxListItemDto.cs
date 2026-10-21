namespace MyLeitner.Application.Boxes.Queries.Models;

public record BoxListItemDto
{
    public long Id { get; set; }
    public short OrderBox { get; set; }
    public string Name { get; set; } = null!;
    public DateTime? StartDate { get; set; }
    public int TotalFlashcardCount { get; set; }
}