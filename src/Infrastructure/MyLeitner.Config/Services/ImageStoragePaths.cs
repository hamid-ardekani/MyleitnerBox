using Microsoft.Extensions.Configuration;

namespace MyLeitner.Config.Services;

/// <summary>
/// تنها منبع مسیر ذخیرهٔ تصویرهای بلوکی فلش‌کارت روی دیسک، تا ثبت DI (که فایل می‌نویسد) و
/// static-file middleware (که <c>/uploads/flashcards</c> را سرو می‌کند) از هم جدا نشوند.
/// </summary>
public static class ImageStoragePaths
{
    /// <summary>
    /// "ImageStorage:RootPath" در appsettings اجازه می‌دهد production پوشه‌ای بیرون از پوشهٔ deploy (که با redeploy پاک نشود) بدهد، بدون تغییر کد.
    /// در غیر این صورت پوشه‌ای کنار برنامه استفاده می‌شود که برای dev محلی کافی است.
    /// </summary>
    public static string ResolveUploadsRootPath(IConfiguration configuration)
    {
        var configured = configuration["ImageStorage:RootPath"];
        return string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads", "flashcards")
            : configured;
    }
}
