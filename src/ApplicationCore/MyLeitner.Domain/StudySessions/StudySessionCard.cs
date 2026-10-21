using Dela.Toolkit.Domain;
using MyLeitner.Domain.Flashcards;

namespace MyLeitner.Domain.StudySessions;

public enum StudySessionCardStatus
{
    Deferred = 0,
    Answered = 1,
    /// <summary>«بلدمش» - از مرور کنار گذاشته شد، بدون پاسخ.</summary>
    SetAside = 2
}

/// <summary>یک بار نمایش یک فلش‌کارت در یک <see cref="StudySession"/> که به سرانجام رسیده: پاسخ (Answered) یا «بعداً نشان بده» (Deferred).</summary>
public class StudySessionCard : Entity<long>
{
    public long StudySessionId { get; private set; }
    public long FlashcardId { get; private set; }
    public long BoxId { get; private set; }
    public StudySessionCardStatus Status { get; private set; }
    public short? AnswerTypeId { get; private set; }
    public DateTime ShownAt { get; private set; }
    public DateTime ResolvedAt { get; private set; }

    /// <summary>کارت آن لحظه «جدید» بود (هیچ‌وقت پاسخ نگرفته بود) - برای شمارش «۵ مرور تازه از آخرین کارت جدید».</summary>
    public bool WasNew { get; private set; }

    public static StudySessionCard ForAnswered(long studySessionId, long flashcardId, long boxId, AnswerTypeEnum answerType, DateTime shownAt, DateTime resolvedAt, bool wasNew = false)
    {
        return new StudySessionCard
        {
            StudySessionId = studySessionId,
            FlashcardId = flashcardId,
            BoxId = boxId,
            Status = StudySessionCardStatus.Answered,
            WasNew = wasNew,
            AnswerTypeId = answerType.Id,
            ShownAt = shownAt,
            ResolvedAt = resolvedAt
        };
    }

    public static StudySessionCard ForSetAside(long studySessionId, long flashcardId, long boxId, DateTime shownAt, DateTime resolvedAt)
    {
        return new StudySessionCard
        {
            StudySessionId = studySessionId,
            FlashcardId = flashcardId,
            BoxId = boxId,
            Status = StudySessionCardStatus.SetAside,
            ShownAt = shownAt,
            ResolvedAt = resolvedAt
        };
    }

    public static StudySessionCard ForDeferred(long studySessionId, long flashcardId, long boxId, DateTime shownAt, DateTime resolvedAt, bool wasNew = false)
    {
        return new StudySessionCard
        {
            StudySessionId = studySessionId,
            FlashcardId = flashcardId,
            BoxId = boxId,
            Status = StudySessionCardStatus.Deferred,
            WasNew = wasNew,
            ShownAt = shownAt,
            ResolvedAt = resolvedAt
        };
    }
}
