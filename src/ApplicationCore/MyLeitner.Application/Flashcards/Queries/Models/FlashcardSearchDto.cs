namespace MyLeitner.Application.Flashcards.Queries.Models;

/// <summary>نتیجهٔ جستجو: تعداد کل و تعداد جعبه‌ها (برای «۴ کارت در ۲ جعبه») + یک صفحه از کارت‌ها.</summary>
public record FlashcardSearchResultDto(int Total, int BoxCount, List<FlashcardSearchItemDto> Items);

/// <param name="Front">متن سادهٔ جلو (کوتاه‌شده).</param>
/// <param name="BackSnippet">تکه‌ای از متن پشت حول اولین کلمهٔ پیدا شده (یا ابتدای آن).</param>
/// <param name="AddedToReviewToday">کارت جدیدی که امروز دستی به مرور اضافه شده.</param>
/// <param name="InFront">کلمه‌ای از جستجو در جلو پیدا شد (همین‌طور InBack / InTags).</param>
public record FlashcardSearchItemDto(
    long Id,
    long BoxId,
    string BoxName,
    string Front,
    string BackSnippet,
    List<string> Tags,
    bool InFront,
    bool InBack,
    bool InTags,
    int Status,
    int Step,
    bool AddedToReviewToday);
