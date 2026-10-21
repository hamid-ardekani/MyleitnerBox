namespace MyLeitner.Application.Flashcards.Queries.Models;

/// <param name="Front">متن سادهٔ جلو (کوتاه‌شده).</param>
public record SimilarFlashcardDto(long Id, long BoxId, string BoxName, string Front);
