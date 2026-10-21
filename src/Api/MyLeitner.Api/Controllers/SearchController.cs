using Dela.Toolkit.Application;
using Microsoft.AspNetCore.Mvc;
using MyLeitner.Application.Flashcards.Queries;
using MyLeitner.Application.Flashcards.Queries.Models;

namespace MyLeitner.Api.Controllers;

public class SearchController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
    : LeitnerController(queryDispatcher, commandDispatcher)
{
    /// <param name="text">کلمه‌ها با فاصله؛ همه باید پیدا شوند.</param>
    /// <param name="fields">front,back,tags (هر ترکیبی)؛ پیش‌فرض front,back.</param>
    /// <param name="boxIds">شناسهٔ جعبه‌ها با کاما؛ خالی = همه.</param>
    [HttpGet]
    public async Task<ActionResult> SearchAsync(
        [FromQuery] string? text,
        [FromQuery] string? fields,
        [FromQuery] string? boxIds,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var chosen = (fields ?? "front,back").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(f => f.ToLowerInvariant()).ToHashSet();
        var boxes = (boxIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => long.TryParse(id, out var v) ? v : 0).Where(v => v > 0).ToList();

        var query = new SearchFlashcardsQuery(_userId, text ?? "", chosen.Contains("front"), chosen.Contains("back"), chosen.Contains("tags"), boxes, skip, take);
        var result = await QueryDispatcher.Dispatch<SearchFlashcardsQuery, FlashcardSearchResultDto>(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>کارت‌های مشابه یک سؤال (فقط جلو، همهٔ جعبه‌ها، حداکثر ۳) برای هشدار هنگام افزودن/ویرایش.</summary>
    /// <param name="text">متن ساده سؤال.</param>
    /// <param name="excludeFlashcardId">کارتی که ویرایش می‌شود؛ در نتیجه نمی‌آید.</param>
    [HttpGet("Similar")]
    public async Task<ActionResult> SimilarAsync(
        [FromQuery] string? text,
        [FromQuery] long? excludeFlashcardId,
        CancellationToken cancellationToken = default)
    {
        var query = new FindSimilarFlashcardsQuery(_userId, text ?? "", excludeFlashcardId);
        var result = await QueryDispatcher.Dispatch<FindSimilarFlashcardsQuery, List<SimilarFlashcardDto>>(query, cancellationToken);
        return Ok(result);
    }
}
