using Dela.Toolkit.Application;

namespace MyLeitner.Application.Contract.Commands;

/// <summary>ذخیرهٔ هدف‌های مرور (روزانه، هفتگی، ماهانه) کاربر؛ بار اول می‌سازد و بعد از آن به‌روز می‌کند.</summary>
public record SaveReviewGoalsCommand(long UserId, int Daily, int Weekly, int Monthly) : ICommand;
