namespace MyLeitner.Application.Boxes.Queries.Models;

/// <param name="Status">LearningStatus: 0 = در مرور، 1 = یادگرفته‌شده، 2 = بایگانی.</param>
/// <param name="Step">گام لایتنری (۰ جدید، ۱..۸) - برای یادگرفته‌شده/بایگانی معنایی ندارد.</param>
public record FlashcardListItemDto(long Id, short Order, string Front, string Back, List<string> Tags, int Status, int Step, bool AddedToReviewToday);
