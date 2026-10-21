using MyLeitner.Application.Contract.Dtos;
using MyLeitner.Application.Contract.ReadModels;

namespace MyLeitner.Application.Contract.ReadStores;

public interface ICardReadRepository
{
    Task<IReadOnlyList<CardReadModel>> GetDueCardsInCategory(long categoryId, CancellationToken ct);
    Task<IReadOnlyList<CardReadModel>> GetDueCardsInBox(long boxId, CancellationToken ct);

    Task<CardDto?> GetCardContentAsync(long flashcardId, CancellationToken ct);

    /// <summary>متن کارت‌های «دور دوم» - فقط آن‌هایی که هنوز در مرورند (نه یادگرفته/بایگانی).</summary>
    Task<IReadOnlyList<PracticeCardReadModel>> GetPracticeCardsAsync(IReadOnlyCollection<long> flashcardIds, CancellationToken ct);

    /// <summary>کارت‌هایی که آخرین جواب امروزشان ضعیف بوده (از ReviewLogs)؛ <paramref name="boxIds"/> null = همهٔ جعبه‌های کاربر.</summary>
    Task<IReadOnlyList<WeakAnswerReadModel>> GetWeakAnswersTodayAsync(long userId, IReadOnlyCollection<long>? boxIds, DateOnly today, CancellationToken ct);

    Task<IReadOnlyList<CategoryBoxReadModel>> GetCategoryBoxesAsync(long userId, CancellationToken ct);

    /// <summary>جعبه‌های یک مرور: خودِ جعبه، یا جعبه‌های یک دسته‌بندی (به ترتیب جعبه‌ها).</summary>
    Task<IReadOnlyList<ScopeBoxReadModel>> GetScopeBoxesAsync(bool isCategory, long scopeId, CancellationToken ct);

    Task<IReadOnlyList<AnsweredTodayReadModel>> GetAnsweredTodayAsync(IReadOnlyCollection<long> boxIds, DateOnly today, CancellationToken ct);

    /// <summary>برنامهٔ امروزِ جعبه‌های یک مرور (سهم کارت جدید + سهم بدهی)، به کلید BoxId.</summary>
    Task<IReadOnlyDictionary<long, BoxTodayPlanDto>> GetBoxPlansAsync(bool isCategory, long scopeId, CancellationToken ct);
}
