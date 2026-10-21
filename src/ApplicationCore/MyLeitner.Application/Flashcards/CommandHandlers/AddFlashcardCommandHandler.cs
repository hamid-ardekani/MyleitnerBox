using Dela.Toolkit.Application;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Application.Flashcards;
using MyLeitner.Domain.Common;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.Users;

namespace MyLeitner.Application.Flashcards.CommandHandlers;

public class AddFlashcardCommandHandler : ICommandHandler<AddFlashcardCommand>
{
    private readonly IUserDomainService _userService;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IFlashcardContentImageResolver _imageResolver;
    private readonly ITransactionalUnitOfWork _unitOfWork;

    public AddFlashcardCommandHandler(IUserDomainService userService, IFlashcardRepository flashcardRepository, IFlashcardContentImageResolver imageResolver, ITransactionalUnitOfWork unitOfWork)
    {
        _userService = userService;
        _flashcardRepository = flashcardRepository;
        _imageResolver = imageResolver;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddFlashcardCommand command, CancellationToken cancellationToken)
    {
        if (!await _userService.CanUserAddFlashcardToBoxAsync(command.UserId, command.BoxId))
            return;

        var maxOrder = await _flashcardRepository.GetMaxOrderByBoxIdAsync(command.BoxId,cancellationToken);

        var order = (short)(maxOrder + 1);

        // تصویرهای هنوز base64 (تازه انتخاب‌شده در ویرایشگر) ذخیره می‌شوند و content با URL جایگزین می‌شود.
        var front = await _imageResolver.PersistEmbeddedImagesAsync(command.Front, cancellationToken);
        var back = await _imageResolver.PersistEmbeddedImagesAsync(command.Back, cancellationToken);

        var flashcard = Flashcard.ForAdd(command.BoxId, order, front, back, command.FrontSearchText, command.BackSearchText, command.Tags);

        await _flashcardRepository.AddAsync(flashcard);

        // Id کارت را دیتابیس می‌سازد؛ MarkCreated باید بعد از ذخیره بیاید، وگرنه رویداد با Id=0 ساخته می‌شود
        // و LearningSchedule آن به FK می‌خورد. هر دو ذخیره در یک تراکنش‌اند تا کارت بدون برنامهٔ مرور نماند.
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _unitOfWork.CommitAsync(cancellationToken);

            flashcard.MarkCreated(command.UserId);

            await _unitOfWork.CommitAsync(cancellationToken);
        }, cancellationToken);
    }
}