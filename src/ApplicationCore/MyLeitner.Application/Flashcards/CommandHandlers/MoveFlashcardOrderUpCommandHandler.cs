using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands; 
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.Flashcards.Exceptions;

namespace MyLeitner.Application.Flashcards.CommandHandlers;

public class MoveFlashcardOrderUpCommandHandler: ICommandHandler<MoveFlashcardOrderUpCommand>
{
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MoveFlashcardOrderUpCommandHandler(IFlashcardRepository flashcardRepository, IUnitOfWork unitOfWork)
    {
        _flashcardRepository = flashcardRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MoveFlashcardOrderUpCommand command, CancellationToken cancellationToken)
    {
        var flashcard = await _flashcardRepository.GetByIdAsync(command.FlashcardId, cancellationToken);
        if (flashcard == null)
            throw new FlashcardNotFoundException(command.FlashcardId);
        
        if(flashcard.Order == 1)
            return;
        
        var currentOrder = flashcard.Order;
        
        var upperFlashcard = await _flashcardRepository.GetByOrderAsync((short)(currentOrder - 1), cancellationToken); 
        if (upperFlashcard == null)
            throw new FlashcardNotFoundException(currentOrder);

        flashcard.UpdateOrder((short)(currentOrder - 1));
        upperFlashcard.UpdateOrder(currentOrder);
 
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}