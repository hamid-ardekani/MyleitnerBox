using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.Flashcards.Exceptions;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.LearningSchedules.Exceptions;
using MyLeitner.Domain.ReviewLogs;
using MyLeitner.Domain.StudySessions;
using AnswerRateTimeType = MyLeitner.Application.Learnings.Enums.AnswerRateTimeType;

namespace MyLeitner.Application.Learnings.Commands;

public record AnswerProcessCommand(long FlashcardId, AnswerRateTimeType AnswerRateTimeType, int ResponseTimeInSeconds, long? CategoryId, long UserId) : ICommand;

public class AnswerProcessCommandHandler : ICommandHandler<AnswerProcessCommand>
{
    private readonly ILearningScheduleRepository _repository;
    private readonly IFlashcardRepository _flashcards;
    private readonly IStudySessionRepository _sessions;
    private readonly IReviewLogRepository _reviewLogs;
    private readonly IUnitOfWork _unitOfWork;

    public AnswerProcessCommandHandler(ILearningScheduleRepository repository, IFlashcardRepository flashcards, IStudySessionRepository sessions, IReviewLogRepository reviewLogs, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _flashcards = flashcards;
        _sessions = sessions;
        _reviewLogs = reviewLogs;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AnswerProcessCommand command, CancellationToken cancellationToken)
    {
        var learningSchedule = await _repository.GetByFlashcardIdAsync(command.FlashcardId, cancellationToken)
                                ?? throw new LearningScheduleNotFoundException(command.FlashcardId);

        // TODO: userGlobalMedian

        var answerTypeEnum = AnswerTypeEnum.Create((short)command.AnswerRateTimeType);

        var previousAnsweredAt = learningSchedule.LastAnswerDate;
        var dueAt = learningSchedule.NextAnswerDate;
        var before = (learningSchedule.Stability, learningSchedule.Difficulty, learningSchedule.Interval);

        learningSchedule.AnswerProcess(answerTypeEnum, command.ResponseTimeInSeconds, LearningSchedule.DefaultUserGlobalMedianSeconds); //,command.UserId

        var flashcard = await _flashcards.GetByIdAsync(command.FlashcardId, cancellationToken)
                         ?? throw new FlashcardNotFoundException(command.FlashcardId);

        var now = DateTime.UtcNow;
        var today = StudySessionClock.LocalToday(now);

        var scopeType = command.CategoryId.HasValue ? StudySessionScopeType.Category : StudySessionScopeType.Box;
        var scopeId = command.CategoryId ?? flashcard.BoxId;

        var session = await _sessions.GetAsync(command.UserId, scopeType, scopeId, today, cancellationToken)
                      ?? throw new InvalidOperationException("هیچ نشست مروری برای امروز پیدا نشد.");

        session.ResolveCurrentAsAnswered(command.FlashcardId, flashcard.BoxId, answerTypeEnum, now, now, wasNew: previousAnsweredAt == null);

        await _reviewLogs.AddAsync(ReviewLog.ForAnswer(
            command.UserId, command.FlashcardId, flashcard.BoxId, command.CategoryId,
            answerTypeEnum, command.ResponseTimeInSeconds,
            now, today, previousAnsweredAt, dueAt,
            before,
            (learningSchedule.Stability, learningSchedule.Difficulty, learningSchedule.Interval)), cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}