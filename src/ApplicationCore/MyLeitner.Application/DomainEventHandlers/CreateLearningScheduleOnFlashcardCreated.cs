using MyLeitner.Application.Contract.EventDispatcher;
using MyLeitner.Domain.Flashcards.Events;
using MyLeitner.Domain.LearningSchedules;

namespace MyLeitner.Application.DomainEventHandlers;

public class CreateLearningScheduleOnFlashcardCreated : IDomainEventHandler<FlashcardCreatedEvent>
{
    private readonly ILearningScheduleRepository _learningScheduleRepository;

    public CreateLearningScheduleOnFlashcardCreated(ILearningScheduleRepository learningScheduleRepository)
    {
        _learningScheduleRepository = learningScheduleRepository;
    }

    public async Task Handle(FlashcardCreatedEvent domainEvent, CancellationToken ct)
    {
        var ls = LearningSchedule.ForAdd(domainEvent.UserId, domainEvent.BoxId, domainEvent.FlashcardId);
        await _learningScheduleRepository.AddAsync(ls);
    }
}