namespace MyLeitner.Application.Stats.Queries.Models;

/// <summary>صفحهٔ «خانه»: چند عدد و جمله از پیشرفت کاربر برای انگیزه دادن، بدون نمودار؛ همهٔ جعبه‌ها.</summary>
/// <param name="Progress">امروز / هفته / ماه در برابر هدف و گذشتهٔ خود کاربر (همان <see cref="ProgressDto"/>).</param>
/// <param name="Streak">روزهای پشت‌سرهم تا امروز؛ اگر امروز هنوز مرور نشده از دیروز حساب می‌شود.</param>
/// <param name="BestStreak">طولانی‌ترین زنجیرهٔ روزهای پشت‌سرهم تا حالا.</param>
/// <param name="TotalAnswers">همهٔ جواب‌های ثبت‌شده (از شروع تاریخچه).</param>
/// <param name="NextMilestone">اولین عدد رند بالاتر از <paramref name="TotalAnswers"/> (۱۰۰، ۵۰۰، ۱۰۰۰، ۲۰۰۰، ...).</param>
/// <param name="Mastered">کارت‌های یادگرفته‌شدهٔ همهٔ جعبه‌ها.</param>
/// <param name="StepsUpWeek">جواب‌های این هفته (شنبه تا امروز) که کارت را حداقل یک گام جلو بردند.</param>
/// <param name="RemainingToday">مرور و کارت جدیدی که امروز در صف مانده (همه‌ی جعبه‌ها).</param>
/// <param name="Boxes">جعبه‌هایی که امروز کار دارند، پرکارترین اول.</param>
/// <param name="Categories">دسته‌هایی که امروز کار دارند، پرکارترین اول؛ همان شکل <paramref name="Boxes"/> با CategoryId.</param>
/// <param name="Resume">مقصد دکمهٔ «ادامهٔ مرور»: جعبه/دسته‌ای که آخرین جواب در آن بود، اگر هنوز کار دارد؛ وگرنه null.</param>
public record HomeDto(
    ProgressDto Progress,
    int Streak,
    int BestStreak,
    int TotalAnswers,
    int NextMilestone,
    int Mastered,
    int StepsUpWeek,
    int RemainingToday,
    List<HomeBoxDto> Boxes,
    List<HomeCategoryDto> Categories,
    HomeResumeDto? Resume);

/// <param name="Remaining">مرورهای امروز + کارت‌های جدیدی که امروز وارد مرور می‌شوند.</param>
public record HomeBoxDto(long BoxId, string Name, int Remaining);

/// <param name="Kind">"box" یا "category" - مسیر مرور <c>/study/{kind}/{id}</c>.</param>
/// <param name="Remaining">کارت‌های مانده امروز در این جعبه، یا جمع جعبه‌های این دسته.</param>
public record HomeResumeDto(string Kind, long Id, string Name, int Remaining);

/// <param name="Remaining">جمع مانده‌های امروزِ جعبه‌های این دسته.</param>
public record HomeCategoryDto(long CategoryId, string Name, int Remaining);
