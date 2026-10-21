using Dela.Toolkit.Domain;
using MyLeitner.Domain.Flashcards;

namespace MyLeitner.Domain.StudySessions;

public enum StudySessionScopeType
{
    Category = 0,
    Box = 1
}

/// <summary>«یک روز مرور» یک کاربر روی یک دسته‌بندی یا جعبه.</summary>
/// <remarks>
/// کلید طبیعی: (UserId, ScopeType, ScopeId, SessionDate). با نیمه‌شب محلی تاریخ عوض می‌شود و session جدید ساخته می‌شود.
/// <see cref="CurrentFlashcardId"/> کارتی است که الان نمایش داده شده و هنوز نتیجه‌اش (پاسخ یا «بعداً») ثبت نشده.
/// </remarks>
public class StudySession : AggregateRoot<long>
{
    private readonly List<StudySessionCard> _cards = [];

    public long UserId { get; private set; }
    public StudySessionScopeType ScopeType { get; private set; }
    public long ScopeId { get; private set; }
    public DateOnly SessionDate { get; private set; }
    public long? CurrentFlashcardId { get; private set; }

    public IReadOnlyList<StudySessionCard> Cards => _cards;

    public static StudySession ForAdd(long userId, StudySessionScopeType scopeType, long scopeId, DateOnly sessionDate)
    {
        return new StudySession
        {
            UserId = userId,
            ScopeType = scopeType,
            ScopeId = scopeId,
            SessionDate = sessionDate
        };
    }

    /// <summary>این فلش‌کارت الان به کاربر نمایش داده می‌شود.</summary>
    public void ShowFlashcard(long flashcardId)
    {
        CurrentFlashcardId = flashcardId;
    }

    /// <summary>کاربر به فلش‌کارتِ در حال نمایش پاسخ داد.</summary>
    public void ResolveCurrentAsAnswered(long flashcardId, long boxId, AnswerTypeEnum answerType, DateTime shownAt, DateTime resolvedAt, bool wasNew = false)
    {
        EnsureCurrentCardIs(flashcardId);

        _cards.Add(StudySessionCard.ForAnswered(Id, flashcardId, boxId, answerType, shownAt, resolvedAt, wasNew));
        CurrentFlashcardId = null;
    }

    /// <summary>«بعداً نشان بده» برای کارت در حال نمایش؛ بعد از بقیهٔ کارت‌های امروز دوباره واجد شرایط می‌شود.</summary>
    public void ResolveCurrentAsDeferred(long flashcardId, long boxId, DateTime shownAt, DateTime resolvedAt, bool wasNew = false)
    {
        EnsureCurrentCardIs(flashcardId);

        _cards.Add(StudySessionCard.ForDeferred(Id, flashcardId, boxId, shownAt, resolvedAt, wasNew));
        CurrentFlashcardId = null;
    }

    /// <summary>«بلدمش» برای کارت در حال نمایش؛ از مرور کنار گذاشته می‌شود (نه پاسخ، نه «بعداً»).</summary>
    public void ResolveCurrentAsSetAside(long flashcardId, long boxId, DateTime shownAt, DateTime resolvedAt)
    {
        EnsureCurrentCardIs(flashcardId);

        _cards.Add(StudySessionCard.ForSetAside(Id, flashcardId, boxId, shownAt, resolvedAt));
        CurrentFlashcardId = null;
    }

    /// <summary>مطمئن می‌شود همان کارتی که کلاینت می‌گوید در این session نمایش داده شده؛ جلوی resolve کارت اشتباه (FlashcardId کهنه) را می‌گیرد.</summary>
    private void EnsureCurrentCardIs(long flashcardId)
    {
        if (CurrentFlashcardId is null)
            throw new InvalidOperationException("هیچ فلش‌کارتی در حال نمایش نیست.");

        if (CurrentFlashcardId != flashcardId)
            throw new InvalidOperationException($"فلش‌کارت {flashcardId} الان در این session نمایش داده نمی‌شود.");
    }
}