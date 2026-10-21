using Dela.Toolkit.Domain;

namespace MyLeitner.Domain.Flashcards.Events;

public class FlashcardCreatedEvent(long flashcardId, long boxId, long userId) : DomainEvent
{
    public long FlashcardId { get; set; } = flashcardId;

    public long BoxId { get; set; } = boxId;

    public long UserId { get; set; } = userId;
}