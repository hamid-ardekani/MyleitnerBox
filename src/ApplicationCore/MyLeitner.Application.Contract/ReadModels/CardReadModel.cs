namespace MyLeitner.Application.Contract.ReadModels;

/// <summary>
/// یک فلش‌کارت موعدرسیده، خوانده‌شده از <c>ReadModelDbContext</c>، آمادهٔ تبدیل به
/// <see cref="MyLeitner.Domain.StudySessions.CardCandidate"/> برای انتخاب در یک session.
/// </summary>
public class CardReadModel
{
    public long CardId { get; set; }
    public long BoxId { get; set; }
    public DateTime NextReviewAt { get; set; }
    public double Stability { get; set; }
    public double Difficulty { get; set; }

    /// <summary>فاصلهٔ فعلی مرور (روز) - برای گام لایتنری.</summary>
    public double Interval { get; set; }

    /// <summary>هنوز هیچ‌وقت پاسخ نگرفته.</summary>
    public bool IsNew { get; set; }

    /// <summary>کارت جدیدی که کاربر امروز دستی به مرور اضافه کرده - بدون انتظار جایزه/سهم می‌آید.</summary>
    public bool AddedToReviewToday { get; set; }
}

public record ScopeBoxReadModel(long BoxId, string Name);

/// <param name="Step">گامی که کارت الان (بعد از جواب امروز) در آن است؛ ۹ = یادگرفته‌شده.</param>
/// <param name="WrongAnswers">تعداد پاسخ‌های «اشتباه» امروز به این کارت - فقط مرورها، نه اولین جواب کارت جدید.</param>
public record AnsweredTodayReadModel(long FlashcardId, long BoxId, int Step, int WrongAnswers);

/// <summary>کارتی که آخرین جواب امروزش «بلد نبودم» (4) یا «شانسی» (3) بوده و هنوز در مرور است.</summary>
public sealed record WeakAnswerReadModel(long FlashcardId, long BoxId, short AnswerTypeId);

public sealed record CategoryBoxReadModel(long CategoryId, long BoxId);

/// <summary>یک کارت «دور دوم» با سختی‌اش (برای ترتیب «سخت‌ترها اول»).</summary>
public sealed record PracticeCardReadModel(long CardId, long BoxId, string BoxName, string Front, string Back, double Difficulty);
