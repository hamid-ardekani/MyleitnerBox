using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyLeitner.Application.Boxes.Queries.Models;
using MyLeitner.Application.Flashcards.Queries;
using MyLeitner.Application.Flashcards.Queries.Models;
using MyLeitner.Domain.Flashcards.Exceptions;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.StudySessions;
using MyLeitner.Persistence.ReadModel.Sql.Contexts;

namespace MyLeitner.Persistence.ReadModel.Sql.Service;

public class FlashcardReadService(ReadModelDbContext context) : ReadService(context), IFlashcardReadService
{
    public async Task<List<FlashcardListItemDto>> GetFlashcardsByBoxIdAsync(long boxId, int page, int limit, CancellationToken cancellationToken)
    {
        var todayStart = StudySessionClock.UtcStartOf(StudySessionClock.LocalToday(DateTime.UtcNow));
        var tomorrowStart = todayStart.AddDays(1);

        return await Context.Flashcards
            .Where(c => c.BoxId == boxId)
            .OrderBy(c => c.Order)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(c => new FlashcardListItemDto(
                c.Id,
                c.Order,
                c.Front,
                c.Back,
                c.Tags.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
                c.LearningSchedule != null ? c.LearningSchedule.Status : 0,
                LearningStep.For(c.LearningSchedule == null || c.LearningSchedule.NextAnswerDate == null,
                    c.LearningSchedule != null ? c.LearningSchedule.Interval : 0),
                c.LearningSchedule != null && c.LearningSchedule.Status == 0 && c.LearningSchedule.NextAnswerDate == null
                    && c.LearningSchedule.AddedToReviewOn >= todayStart && c.LearningSchedule.AddedToReviewOn < tomorrowStart))
            .ToListAsync(cancellationToken);
    }

    public async Task<FlashcardForSaveDto> GetFlashcardByIdAsync(long flashcardId, CancellationToken cancellationToken)
    {
        var flashcard = await Context.Flashcards.FirstOrDefaultAsync(f => f.Id == flashcardId, cancellationToken);

        if (flashcard == null)
            throw new FlashcardNotFoundException(flashcardId);

        return new FlashcardForSaveDto(flashcard.Id, flashcard.Front, flashcard.Back,
            flashcard.FrontSearchText ?? string.Empty, flashcard.BackSearchText ?? string.Empty,
            flashcard.Tags.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList());
    }

    private const int FrontMaxLength = 200;
    private const int SnippetLength = 140;
    private const int MaxTake = 100;

    public async Task<FlashcardSearchResultDto> SearchAsync(SearchFlashcardsQuery query, CancellationToken cancellationToken)
    {
        var words = SearchText.Words(query.Text);
        if (words.Count == 0 || (!query.InFront && !query.InBack && !query.InTags))
            return new FlashcardSearchResultDto(0, 0, []);

        var cards = Context.Flashcards.Where(c => c.Box.UserId == query.UserId);
        if (query.BoxIds.Count > 0)
            cards = cards.Where(c => query.BoxIds.Contains(c.BoxId));

        var inFront = query.InFront;
        var inBack = query.InBack;
        var inTags = query.InTags;

        // همهٔ کلمه‌ها باید پیدا شوند، هر کدام در یکی از فیلدهای انتخاب‌شده؛ ستون مثل کلمه‌ها یکسان‌سازی می‌شود
        // (ي/ك عربی، حذف نیم‌فاصله، حروف کوچک) - فقط اعراب سمت کلمه برداشته می‌شود
        foreach (var word in words)
        {
            cards = cards.Where(c =>
                (inFront && (c.FrontSearchText ?? "").Replace(SearchText.ArabicYeh, SearchText.PersianYeh).Replace(SearchText.AlefMaksura, SearchText.PersianYeh)
                    .Replace(SearchText.ArabicKaf, SearchText.PersianKaf).Replace(SearchText.Zwnj, "").ToLower().Contains(word)) ||
                (inBack && (c.BackSearchText ?? "").Replace(SearchText.ArabicYeh, SearchText.PersianYeh).Replace(SearchText.AlefMaksura, SearchText.PersianYeh)
                    .Replace(SearchText.ArabicKaf, SearchText.PersianKaf).Replace(SearchText.Zwnj, "").ToLower().Contains(word)) ||
                (inTags && c.Tags.Replace(SearchText.ArabicYeh, SearchText.PersianYeh).Replace(SearchText.AlefMaksura, SearchText.PersianYeh)
                    .Replace(SearchText.ArabicKaf, SearchText.PersianKaf).Replace(SearchText.Zwnj, "").ToLower().Contains(word)));
        }

        var total = await cards.CountAsync(cancellationToken);
        if (total == 0)
            return new FlashcardSearchResultDto(0, 0, []);

        var boxCount = await cards.Select(c => c.BoxId).Distinct().CountAsync(cancellationToken);

        var rows = await cards
            .OrderBy(c => c.Box.Name).ThenBy(c => c.BoxId).ThenBy(c => c.Order).ThenBy(c => c.Id)
            .Skip(Math.Max(0, query.Skip))
            .Take(Math.Clamp(query.Take, 1, MaxTake))
            .Select(c => new
            {
                c.Id,
                c.BoxId,
                BoxName = c.Box.Name,
                c.FrontSearchText,
                c.BackSearchText,
                c.Tags,
                Status = c.LearningSchedule != null ? c.LearningSchedule.Status : 0,
                IsNew = c.LearningSchedule == null || c.LearningSchedule.NextAnswerDate == null,
                Interval = c.LearningSchedule != null ? c.LearningSchedule.Interval : 0,
                AddedToReviewOn = c.LearningSchedule != null ? c.LearningSchedule.AddedToReviewOn : null,
            })
            .ToListAsync(cancellationToken);

        var todayStart = StudySessionClock.UtcStartOf(StudySessionClock.LocalToday(DateTime.UtcNow));
        var tomorrowStart = todayStart.AddDays(1);

        var items = rows.Select(r =>
        {
            var front = r.FrontSearchText ?? string.Empty;
            var back = r.BackSearchText ?? string.Empty;
            var normalizedFront = SearchText.Normalize(front);
            var normalizedBack = SearchText.Normalize(back);
            var normalizedTags = SearchText.Normalize(r.Tags);

            return new FlashcardSearchItemDto(
                r.Id,
                r.BoxId,
                r.BoxName,
                front.Length > FrontMaxLength ? front[..FrontMaxLength] + "…" : front,
                Snippet(back, normalizedBack, words),
                r.Tags.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                inFront && words.Any(normalizedFront.Contains),
                inBack && words.Any(normalizedBack.Contains),
                inTags && words.Any(normalizedTags.Contains),
                r.Status,
                LearningStep.For(r.IsNew, r.Interval),
                r.IsNew && r.Status == 0 && r.AddedToReviewOn >= todayStart && r.AddedToReviewOn < tomorrowStart);
        }).ToList();

        return new FlashcardSearchResultDto(total, boxCount, items);
    }

    private const int SimilarTake = 3;
    private const double SimilarShare = 0.7;

    /// <summary>
    /// کارت‌های مشابه یک سؤال: جلوی کارت‌هایی که دست‌کم ۷۰٪ کلمه‌های معنادار سؤال را دارند (نه لزوماً همه). امتیاز هر کارت = تعداد کلمه‌های
    /// پیداشده، که در خود SQL با یک عبارت جمع حساب می‌شود (یک اسکن، بدون ریختن کارت‌ها در حافظه)؛ بیشترین امتیاز و بعد کوتاه‌ترین جلو اول.
    /// </summary>
    public async Task<List<SimilarFlashcardDto>> FindSimilarAsync(FindSimilarFlashcardsQuery query, CancellationToken cancellationToken)
    {
        var words = SearchText.SimilarityWords(query.Text);
        if (words.Count == 0)
            return [];

        var needed = words.Count <= 2 ? words.Count : (int)Math.Ceiling(words.Count * SimilarShare);

        Expression<Func<ReadModels.Flashcard, string>> normalizedFront = c => (c.FrontSearchText ?? "")
            .Replace(SearchText.ArabicYeh, SearchText.PersianYeh).Replace(SearchText.AlefMaksura, SearchText.PersianYeh)
            .Replace(SearchText.ArabicKaf, SearchText.PersianKaf).Replace(SearchText.Zwnj, "").ToLower();
        var card = normalizedFront.Parameters[0];
        var contains = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

        Expression score = Expression.Constant(0);
        foreach (var word in words)
        {
            var found = Expression.Call(normalizedFront.Body, contains, Expression.Constant(word));
            score = Expression.Add(score, Expression.Condition(found, Expression.Constant(1), Expression.Constant(0)));
        }

        var enough = Expression.Lambda<Func<ReadModels.Flashcard, bool>>(Expression.GreaterThanOrEqual(score, Expression.Constant(needed)), card);
        var byScore = Expression.Lambda<Func<ReadModels.Flashcard, int>>(score, card);

        var cards = Context.Flashcards.Where(c => c.Box.UserId == query.UserId);
        if (query.ExcludeFlashcardId is { } excluded)
            cards = cards.Where(c => c.Id != excluded);

        var rows = await cards
            .Where(enough)
            .OrderByDescending(byScore).ThenBy(c => (c.FrontSearchText ?? "").Length).ThenBy(c => c.Id)
            .Take(SimilarTake)
            .Select(c => new { c.Id, c.BoxId, BoxName = c.Box.Name, c.FrontSearchText })
            .ToListAsync(cancellationToken);

        return rows.Select(r =>
        {
            var front = r.FrontSearchText ?? string.Empty;
            return new SimilarFlashcardDto(r.Id, r.BoxId, r.BoxName, front.Length > FrontMaxLength ? front[..FrontMaxLength] + "…" : front);
        }).ToList();
    }

    /// <summary>
    /// تکه‌ای از متن پشت حول اولین کلمهٔ پیدا شده (Normalize طول را عوض نمی‌کند جز حذف اعراب، پس موقعیت تقریبی است).
    /// </summary>
    private static string Snippet(string text, string normalized, List<string> words)
    {
        if (text.Length <= SnippetLength)
            return text;

        var hit = words.Select(w => normalized.IndexOf(w, StringComparison.Ordinal)).Where(i => i >= 0).DefaultIfEmpty(0).Min();
        var start = Math.Clamp(hit - SnippetLength / 3, 0, Math.Max(0, text.Length - SnippetLength));
        var piece = text.Substring(start, Math.Min(SnippetLength, text.Length - start));

        return (start > 0 ? "…" : "") + piece + (start + piece.Length < text.Length ? "…" : "");
    }
}
