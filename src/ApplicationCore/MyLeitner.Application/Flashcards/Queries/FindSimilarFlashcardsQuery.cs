using Dela.Toolkit.Application;
using MyLeitner.Application.Flashcards.Queries.Models;

namespace MyLeitner.Application.Flashcards.Queries;

/// <summary>
/// کارت‌های مشابه یک سؤال هنگام افزودن/ویرایش: فقط متن <b>جلو</b> در همهٔ جعبه‌های کاربر، حداکثر ۳ نتیجه.
/// <paramref name="ExcludeFlashcardId"/> خودِ کارتِ در حال ویرایش است.
/// </summary>
public record FindSimilarFlashcardsQuery(long UserId, string Text, long? ExcludeFlashcardId) : IQuery;

public class FindSimilarFlashcardsQueryHandler(IFlashcardReadService service)
    : IQueryHandler<FindSimilarFlashcardsQuery, List<SimilarFlashcardDto>>
{
    public Task<List<SimilarFlashcardDto>> Handle(FindSimilarFlashcardsQuery query, CancellationToken cancellationToken)
    {
        return service.FindSimilarAsync(query, cancellationToken);
    }
}
