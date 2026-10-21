using Dela.Toolkit.Domain;

namespace MyLeitner.Domain.StudySessions;

/// <summary>
/// یک فلش‌کارتِ موعدرسیده که برای «کارت بعدی» در یک <see cref="StudySession"/> رقابت می‌کند. فقط داده‌های خام
/// <see cref="LearningSchedules.LearningSchedule"/> را دارد (نه Priority آماده)، چون اولویت باید همان لحظهٔ انتخاب حساب شود.
/// </summary>
public class CardCandidate : ValueObject
{
    public long CardId { get; }
    public long BoxId { get; }
    public double Difficulty { get; }
    public double Stability { get; }
    public DateTime NextAnswerDate { get; }

    /// <summary>هنوز هیچ‌وقت پاسخ نگرفته (کارت جدید) - فقط به‌عنوان جایزه وسط مرورها می‌آید.</summary>
    public bool IsNew { get; }

    /// <summary>کارت جدیدی که کاربر امروز دستی به مرور اضافه کرده: مثل مرور عادی می‌آید، نه جایزه.</summary>
    public bool AddedToReviewToday { get; }

    public CardCandidate(long cardId, long boxId, double difficulty, double stability, DateTime nextAnswerDate, bool isNew = false, bool addedToReviewToday = false)
    {
        CardId = cardId;
        BoxId = boxId;
        Difficulty = difficulty;
        Stability = stability;
        NextAnswerDate = nextAnswerDate;
        IsNew = isNew;
        AddedToReviewToday = addedToReviewToday;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        return new object[] { CardId, BoxId, Difficulty, Stability, NextAnswerDate, IsNew, AddedToReviewToday };
    }
}