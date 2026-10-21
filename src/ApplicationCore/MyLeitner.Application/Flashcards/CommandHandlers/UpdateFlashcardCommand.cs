using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Application.Flashcards;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Boxes.Exceptions;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.Flashcards.Exceptions;

namespace MyLeitner.Application.Flashcards.CommandHandlers;

public class UpdateFlashcardCommandHandler : ICommandHandler<UpdateFlashcardCommand>
{
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IBoxRepository _boxRepository;
    private readonly IFlashcardContentImageResolver _imageResolver;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFlashcardCommandHandler(IFlashcardRepository flashcardRepository, IBoxRepository boxRepository, IFlashcardContentImageResolver imageResolver, IUnitOfWork unitOfWork)
    {
        _flashcardRepository = flashcardRepository;
        _boxRepository = boxRepository;
        _imageResolver = imageResolver;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateFlashcardCommand command, CancellationToken cancellationToken)
    {
        var flashcard = await _flashcardRepository.GetByIdAsync(command.FlashcardId, cancellationToken);
        if (flashcard == null)
            throw new FlashcardNotFoundException(command.FlashcardId);

        var front = await _imageResolver.PersistEmbeddedImagesAsync(command.Front, cancellationToken);
        var back = await _imageResolver.PersistEmbeddedImagesAsync(command.Back, cancellationToken);

        flashcard.Update(front, back, command.FrontSearchText, command.BackSearchText, command.Tags);

        if (command.MoveToBoxId is { } targetBoxId && targetBoxId != flashcard.BoxId)
        {
            // فقط به یکی از جعبه‌های خود کاربر، و در انتهای آن
            var target = await _boxRepository.GetByIdAsync(targetBoxId, cancellationToken);
            if (target == null || target.UserId != command.UserId)
                throw new BoxNotFoundException(targetBoxId);

            var maxOrder = await _flashcardRepository.GetMaxOrderByBoxIdAsync(targetBoxId, cancellationToken);
            flashcard.MoveToBox(targetBoxId, (short)(maxOrder + 1));
        }

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}