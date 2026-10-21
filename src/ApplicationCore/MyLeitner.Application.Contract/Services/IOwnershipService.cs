namespace MyLeitner.Application.Contract.Services;

/// <summary>
/// آیا این کاربر مالک این منبع است؟ کنترلرها قبل از هر کار روی منبعی که با شناسه می‌آید (جعبه، دسته، کارت) آن را می‌پرسند،
/// تا کاربری با حدس زدن شناسه به داده‌ی دیگری دست نزند. منبع ناموجود هم «مالک نیست» حساب می‌شود.
/// </summary>
public interface IOwnershipService
{
    Task<bool> OwnsBoxAsync(long userId, long boxId, CancellationToken ct);

    /// <summary>همهٔ جعبه‌ها (بدون تکرار) مال کاربر باشند؛ فهرست خالی درست است.</summary>
    Task<bool> OwnsBoxesAsync(long userId, IReadOnlyCollection<long> boxIds, CancellationToken ct);

    Task<bool> OwnsCategoryAsync(long userId, long categoryId, CancellationToken ct);

    /// <summary>کارت مال کاربر است اگر در جعبهٔ او باشد.</summary>
    Task<bool> OwnsFlashcardAsync(long userId, long flashcardId, CancellationToken ct);
}
