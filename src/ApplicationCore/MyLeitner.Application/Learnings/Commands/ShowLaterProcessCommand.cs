using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.Flashcards.Exceptions;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Application.Learnings.Commands;

public record ShowLaterProcessCommand(long FlashcardId, long? CategoryId, long UserId) : ICommand;

/// <summary>
/// کاربر فعلاً نمی‌خواهد به این فلش‌کارت جواب بدهد؛ در <see cref="StudySession"/> امروز به حالت Deferred می‌رود.
/// زمان‌بندی بلندمدت (<see cref="LearningSchedules.LearningSchedule"/>) تغییر نمی‌کند.
/// </summary>
public class ShowLaterProcessCommandHandler : ICommandHandler<ShowLaterProcessCommand>
{
    private readonly IFlashcardRepository _flashcards;
    private readonly IStudySessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILearningScheduleRepository _schedules;

    public ShowLaterProcessCommandHandler(IFlashcardRepository flashcards, IStudySessionRepository sessions, IUnitOfWork unitOfWork, ILearningScheduleRepository schedules)
    {
        _schedules = schedules;
        _flashcards = flashcards;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ShowLaterProcessCommand command, CancellationToken cancellationToken)
    {
        var flashcard = await _flashcards.GetByIdAsync(command.FlashcardId, cancellationToken)
                         ?? throw new FlashcardNotFoundException(command.FlashcardId);

        var now = DateTime.UtcNow;
        var today = StudySessionClock.LocalToday(now);

        // مرور از داخل دسته‌بندی: session همان دسته است؛ وگرنه session جعبهٔ خودِ کارت
        var scopeType = command.CategoryId.HasValue ? StudySessionScopeType.Category : StudySessionScopeType.Box;
        var scopeId = command.CategoryId ?? flashcard.BoxId;

        var session = await _sessions.GetAsync(command.UserId, scopeType, scopeId, today, cancellationToken)
                      ?? throw new InvalidOperationException("هیچ نشست مروری برای امروز پیدا نشد.");

        // کارت جدیدی که عقب بیفتد پاداشش خرج‌شده حساب می‌شود؛ کارت بعدی باید دوباره کسب شود
        var schedule = await _schedules.GetByFlashcardIdAsync(command.FlashcardId, cancellationToken);
        session.ResolveCurrentAsDeferred(command.FlashcardId, flashcard.BoxId, now, now, wasNew: schedule?.LastAnswerDate == null);

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}