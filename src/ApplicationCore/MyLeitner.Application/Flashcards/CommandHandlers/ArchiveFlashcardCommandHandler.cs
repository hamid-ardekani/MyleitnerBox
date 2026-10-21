using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.LearningSchedules.Exceptions;

namespace MyLeitner.Application.Flashcards.CommandHandlers;

public class ArchiveFlashcardCommandHandler : ICommandHandler<ArchiveFlashcardCommand>
{
    private readonly ILearningScheduleRepository _schedules;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveFlashcardCommandHandler(ILearningScheduleRepository schedules, IUnitOfWork unitOfWork)
    {
        _schedules = schedules;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ArchiveFlashcardCommand command, CancellationToken cancellationToken)
    {
        var schedule = await _schedules.GetByFlashcardIdAsync(command.FlashcardId, cancellationToken)
                       ?? throw new LearningScheduleNotFoundException(command.FlashcardId);

        schedule.Archive();

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}

public class RestoreFlashcardCommandHandler : ICommandHandler<RestoreFlashcardCommand>
{
    private readonly ILearningScheduleRepository _schedules;
    private readonly IUnitOfWork _unitOfWork;

    public RestoreFlashcardCommandHandler(ILearningScheduleRepository schedules, IUnitOfWork unitOfWork)
    {
        _schedules = schedules;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RestoreFlashcardCommand command, CancellationToken cancellationToken)
    {
        var schedule = await _schedules.GetByFlashcardIdAsync(command.FlashcardId, cancellationToken)
                       ?? throw new LearningScheduleNotFoundException(command.FlashcardId);

        schedule.RestoreToLearning();

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}

public class AddFlashcardToReviewCommandHandler : ICommandHandler<AddFlashcardToReviewCommand>
{
    private readonly ILearningScheduleRepository _schedules;
    private readonly IUnitOfWork _unitOfWork;

    public AddFlashcardToReviewCommandHandler(ILearningScheduleRepository schedules, IUnitOfWork unitOfWork)
    {
        _schedules = schedules;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddFlashcardToReviewCommand command, CancellationToken cancellationToken)
    {
        var schedule = await _schedules.GetByFlashcardIdAsync(command.FlashcardId, cancellationToken)
                       ?? throw new LearningScheduleNotFoundException(command.FlashcardId);

        schedule.AddToReview();

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
