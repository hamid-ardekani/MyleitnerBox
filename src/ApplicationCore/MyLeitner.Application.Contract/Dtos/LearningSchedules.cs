namespace MyLeitner.Application.Contract.Dtos;


public enum LearningScheduleStatus
{
    HasFlashcards,
    NoFlashcardsDefined,
    AllFlashcardsCompletedForToday
}

public sealed record StudySessionResult(LearningScheduleStatus Status, LearningFlashcardDto? Flashcard);

public record LearningFlashcardDto(long FlashcardId, long BoxId, string Front, string Back, int Step = 0, int? DaysLate = null, string BoxName = "");

/// <summary>فاصلهٔ مرور بعدی (به روز) اگر کارت با <paramref name="AnswerRateTimeType"/> (مقدار عددی enum) پاسخ داده شود.</summary>
/// <param name="Masters">این پاسخ یادگیری کارت را تمام می‌کند (یادگرفته‌شده می‌شود و از مرور روزانه بیرون می‌رود).</param>
/// <param name="Step">گام لایتنری کارت بعد از این پاسخ (۹ = یادگرفته‌شده).</param>
public record AnswerIntervalPreviewDto(int AnswerRateTimeType, int IntervalDays, bool Masters = false, int Step = 0);
