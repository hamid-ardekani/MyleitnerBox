using Dela.Toolkit.Application;

namespace MyLeitner.Application.Contract.Commands;

public record AddFlashcardCommand(long BoxId, string Front, string Back, string FrontSearchText, string BackSearchText, IReadOnlyList<string> Tags, long UserId) : ICommand;

/// <param name="MoveToBoxId">کارت را به این جعبهٔ کاربر هم منتقل می‌کند؛ null = در جعبهٔ خودش می‌ماند.</param>
public record UpdateFlashcardCommand(long FlashcardId, string Front, string Back, string FrontSearchText, string BackSearchText, IReadOnlyList<string> Tags, long UserId, long? MoveToBoxId = null) : ICommand;

public record MoveFlashcardOrderUpCommand(long BoxId, long FlashcardId) : ICommand;

public record DeleteFlashcardCommand(long FlashcardId):ICommand;

/// <summary>کارت را دستی از مرور روزانه بیرون می‌گذارد (تاریخچه و زمان‌بندی‌اش می‌ماند).</summary>
public record ArchiveFlashcardCommand(long FlashcardId) : ICommand;

/// <summary>کارت بایگانی‌شده یا یادگرفته‌شده را به مرور برمی‌گرداند و موعدش همین امروز است.</summary>
public record RestoreFlashcardCommand(long FlashcardId) : ICommand;

/// <summary>کارت جدید (هیچ‌وقت پاسخ‌نگرفته) را بدون شرط جایزه/سهم به مرور امروز جعبه‌اش اضافه می‌کند.</summary>
public record AddFlashcardToReviewCommand(long FlashcardId) : ICommand;
