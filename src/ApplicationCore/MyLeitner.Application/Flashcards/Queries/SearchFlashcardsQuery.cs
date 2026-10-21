using Dela.Toolkit.Application;
using MyLeitner.Application.Flashcards.Queries.Models;

namespace MyLeitner.Application.Flashcards.Queries;

/// <summary>
/// جستجو در جلو/پشت/برچسب کارت‌های کاربر؛ همهٔ کلمه‌ها باید در یکی از فیلدهای روشن پیدا شوند. کارت‌های بایگانی‌شده
/// و یادگرفته‌شده هم می‌آیند. <paramref name="BoxIds"/> خالی = همهٔ جعبه‌ها.
/// </summary>
public record SearchFlashcardsQuery(
    long UserId,
    string Text,
    bool InFront,
    bool InBack,
    bool InTags,
    IReadOnlyList<long> BoxIds,
    int Skip,
    int Take) : IQuery;

public class SearchFlashcardsQueryHandler(IFlashcardReadService service) : IQueryHandler<SearchFlashcardsQuery, FlashcardSearchResultDto>
{
    public Task<FlashcardSearchResultDto> Handle(SearchFlashcardsQuery query, CancellationToken cancellationToken)
    {
        return service.SearchAsync(query, cancellationToken);
    }
}
