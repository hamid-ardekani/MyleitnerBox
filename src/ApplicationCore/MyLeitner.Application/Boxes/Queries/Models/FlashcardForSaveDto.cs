namespace MyLeitner.Application.Boxes.Queries.Models;
 
public readonly record struct DocumentDto(long Id, string TextBlocks,string ImageBlocks);
/// <param name="MoveToBoxId">هنگام ذخیره: کارت به این جعبه (جعبهٔ دیگری از همان کاربر) منتقل می‌شود؛ null = همان‌جا می‌ماند.</param>
public readonly record struct FlashcardForSaveDto(long FlashcardId, string Front, string Back, string FrontSearchText, string BackSearchText, IReadOnlyList<string> Tags, long? MoveToBoxId = null);