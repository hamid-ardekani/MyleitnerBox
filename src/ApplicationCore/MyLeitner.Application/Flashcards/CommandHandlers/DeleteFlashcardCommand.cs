using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.Flashcards.Exceptions;

namespace MyLeitner.Application.Flashcards.CommandHandlers;

public class DeleteFlashcardCommandHandler : ICommandHandler<DeleteFlashcardCommand>
{
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFlashcardCommandHandler(IFlashcardRepository flashcardRepository, IUnitOfWork unitOfWork)
    {
        _flashcardRepository = flashcardRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteFlashcardCommand command, CancellationToken cancellationToken)
    {
        var flashcardMustBeDelete = await _flashcardRepository.GetByIdAsync(command.FlashcardId, cancellationToken);
        if (flashcardMustBeDelete == null)
            throw new FlashcardNotFoundException(command.FlashcardId);

        _flashcardRepository.Remove(flashcardMustBeDelete);

        var flashcards = await _flashcardRepository.GetAllByBoxIdAsync(flashcardMustBeDelete.BoxId, cancellationToken);
        short order = 1;
        foreach (var flashcard in flashcards)
        {
            if (flashcard.Id == command.FlashcardId)
                continue;

            flashcard.UpdateOrder(order++);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}