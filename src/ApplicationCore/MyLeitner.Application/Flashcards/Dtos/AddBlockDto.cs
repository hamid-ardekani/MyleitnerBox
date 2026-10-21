namespace MyLeitner.Application.Flashcards.Dtos;

public record AddBlockDto(Guid Id,short Order, int BlockType, string Content);