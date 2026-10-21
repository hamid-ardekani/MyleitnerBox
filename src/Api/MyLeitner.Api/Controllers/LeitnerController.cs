using Dela.Toolkit.Application;
using Dela.Toolkit.ServiceHost;
using Microsoft.AspNetCore.Authorization;
using MyLeitner.Application.Contract.Services;

namespace MyLeitner.Api.Controllers;

[Authorize]
public abstract class LeitnerController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
    : BaseController(queryDispatcher, commandDispatcher)
{
    /// <summary>شناسهٔ کاربرِ واردشده از claim «sub» توکن (کلاینت نمی‌تواند آن را عوض کند).</summary>
    protected long _userId =>
        long.TryParse(User.FindFirst("sub")?.Value, out var id) ? id : throw new UnauthorizedAccessException();

    private bool IsSuperUser => User.FindFirst("is_super")?.Value == "true";

    private IOwnershipService Ownership => HttpContext.RequestServices.GetRequiredService<IOwnershipService>();

    // اگر کاربر مالک نباشد NotFound برمی‌گردد (نه Forbidden) تا وجود شناسه لو نرود.
    // کاربر ویژه به همه‌چیز دسترسی دارد.
    protected async Task<bool> NotOwnsBoxAsync(long boxId, CancellationToken ct) =>
        !IsSuperUser && !await Ownership.OwnsBoxAsync(_userId, boxId, ct);

    protected async Task<bool> NotOwnsBoxesAsync(IReadOnlyCollection<long>? boxIds, CancellationToken ct) =>
        !IsSuperUser && boxIds is { Count: > 0 } && !await Ownership.OwnsBoxesAsync(_userId, boxIds, ct);

    protected async Task<bool> NotOwnsCategoryAsync(long categoryId, CancellationToken ct) =>
        !IsSuperUser && !await Ownership.OwnsCategoryAsync(_userId, categoryId, ct);

    protected async Task<bool> NotOwnsFlashcardAsync(long flashcardId, CancellationToken ct) =>
        !IsSuperUser && !await Ownership.OwnsFlashcardAsync(_userId, flashcardId, ct);

    /// <summary>برای ورودی «یا جعبه یا دسته» (Overview/Practice) و «دستهٔ اختیاری» (پاسخ‌ها).</summary>
    protected async Task<bool> NotOwnsBoxOrCategoryAsync(long? boxId, long? categoryId, CancellationToken ct) =>
        (boxId is { } b && await NotOwnsBoxAsync(b, ct)) || (categoryId is { } c && await NotOwnsCategoryAsync(c, ct));
}
