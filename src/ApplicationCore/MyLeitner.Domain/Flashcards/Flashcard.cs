using System.ComponentModel;
using Dela.Toolkit.Domain;
using MyLeitner.Domain.Boxes.Exceptions;
using MyLeitner.Domain.Flashcards.Events;
using MyLeitner.Domain.Flashcards.Exceptions;

namespace MyLeitner.Domain.Flashcards;

public class Flashcard : AggregateRoot<long>
{
    public short Order { get; private set; }
    public string Front { get; private set; } = null!;
    public string Back { get; private set; } = null!;

    /// <summary>متن ساده‌شده از JSON بلوکی <see cref="Front"/> برای جستجو/پیش‌نمایش.</summary>
    public string FrontSearchText { get; private set; } = null!;

    /// <summary>متن ساده‌شده از JSON بلوکی <see cref="Back"/> برای جستجو/پیش‌نمایش.</summary>
    public string BackSearchText { get; private set; } = null!;

    public string Tags { get; private set; } = null!;

    public DateTime? UpdateDate { get; private set; }

    /// <summary>زمان ساخت (UTC). برای کارت‌های ساخته‌شده قبل از اضافه‌شدن این ستون null است.</summary>
    public DateTime? CreatedAt { get; private set; }

    public long BoxId { get; private set; }

    public static Flashcard ForAdd(long boxId, short order, string front, string back, string frontSearchText, string backSearchText, IReadOnlyList<string> tags)
    {
        if (string.IsNullOrEmpty(front) || string.IsNullOrEmpty(front.Trim()))
            throw new FrontNullException();

        if (string.IsNullOrEmpty(back) || string.IsNullOrEmpty(back.Trim()))
            throw new BackNullException();

        var tag = JoinTags(tags);

        var flashcard = new Flashcard
        {
            BoxId = boxId,
            Order = order,
            Front = front,
            Back = back,
            FrontSearchText = frontSearchText ?? string.Empty,
            BackSearchText = backSearchText ?? string.Empty,
            Tags = tag,
            CreatedAt = DateTime.UtcNow
        };

        return flashcard;
    }

    /// <summary>
    /// برچسب‌ها اختیاری‌اند: جستجو در متن کارت انجام می‌شود (۷٬۴۲۳ از ۷٬۴۳۵ کارت فقط نام جعبه‌شان را داشتند).
    /// اپ دیگر آن را نشان نمی‌دهد و نمی‌خواهد؛ مقدارهای موجود در ستون می‌مانند.
    /// </summary>
    private static string JoinTags(IReadOnlyList<string>? tags) =>
        tags == null ? string.Empty : string.Join(",", tags.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()));

    public void MarkCreated(long userId)
    {
        Raise(new FlashcardCreatedEvent(Id, BoxId, userId));
    }

    public void Update(string front, string back, string frontSearchText, string backSearchText, IReadOnlyList<string> tags)
    {
        if (string.IsNullOrEmpty(front) || string.IsNullOrEmpty(front.Trim()))
            throw new FrontNullException();

        if (string.IsNullOrEmpty(back) || string.IsNullOrEmpty(back.Trim()))
            throw new BackNullException();

        var tag = JoinTags(tags);

        Front = front;
        Back = back;
        FrontSearchText = frontSearchText ?? string.Empty;
        BackSearchText = backSearchText ?? string.Empty;
        Tags = tag;
        // آخرین ویرایش محتوا (UTC)؛ جابه‌جایی ترتیب ویرایش حساب نمی‌شود
        UpdateDate = DateTime.UtcNow;
    }

    /// <summary>
    /// کارت را به انتهای جعبهٔ دیگری از همان کاربر منتقل می‌کند. پیشرفت یادگیری (LearningSchedule) دست نمی‌خورد؛
    /// ReviewLogs قبلی جعبهٔ قدیم را نگه می‌دارند.
    /// </summary>
    public void MoveToBox(long boxId, short order)
    {
        if (order <= 0)
            throw new OrderUnValidException();

        BoxId = boxId;
        Order = order;
    }

    public void UpdateOrder(short order)
    {
        if (order <= 0)
            throw new OrderUnValidException();

        Order = order;
    }
}