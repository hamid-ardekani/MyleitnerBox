namespace MyLeitner.Application.Boxes.Queries.Models;

public record BoxForEditDto(string Name, bool ReadFlashcardAloud, short DailyNewCardLimit, short DailyBacklogFloor);
