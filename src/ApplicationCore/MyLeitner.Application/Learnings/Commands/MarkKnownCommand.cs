using Dela.Toolkit.Application;
using Dela.Toolkit.Domain;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.Flashcards.Exceptions;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.LearningSchedules.Exceptions;
using MyLeitner.Domain.StudySessions;

namespace MyLeitner.Application.Learnings.Commands;

public record MarkKnownCommand(long FlashcardId, long? CategoryId, long UserId) : ICommand;

/// <summary>
/// «بلدمش» در مرور: کارت یادگرفته‌شده می‌شود (<see cref="LearningSchedule.MarkKnown"/>) و در session امروز «کنار گذاشته» ثبت می‌شود.
/// پاسخ حساب نمی‌شود.
/// </summary>
public class MarkKnownCommandHandler(
    IFlashcardRepository flashcards,
    ILearningScheduleRepository schedules,
    IStudySessionRepository sessions,
    IUnitOfWork unitOfWork) : ICommandHandler<MarkKnownCommand>
{
    public async Task Handle(MarkKnownCommand command, CancellationToken cancellationToken)
    {
        var flashcard = await flashcards.GetByIdAsync(command.FlashcardId, cancellationToken)
                        ?? throw new FlashcardNotFoundException(command.FlashcardId);
        var schedule = await schedules.GetByFlashcardIdAsync(command.FlashcardId, cancellationToken)
                       ?? throw new LearningScheduleNotFoundException(command.FlashcardId);

        var now = DateTime.UtcNow;
        var today = StudySessionClock.LocalToday(now);
        var scopeType = command.CategoryId.HasValue ? StudySessionScopeType.Category : StudySessionScopeType.Box;
        var scopeId = command.CategoryId ?? flashcard.BoxId;

        schedule.MarkKnown();

        // کارتِ روی صفحه کنار گذاشته می‌شود تا مرور ادامه پیدا کند؛ اگر کارتِ روی صفحه نبود (تب قدیمی) فقط یادگرفته‌شده ثبت می‌شود
        var session = await sessions.GetAsync(command.UserId, scopeType, scopeId, today, cancellationToken);
        if (session?.CurrentFlashcardId == command.FlashcardId)
            session.ResolveCurrentAsSetAside(command.FlashcardId, flashcard.BoxId, now, now);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
