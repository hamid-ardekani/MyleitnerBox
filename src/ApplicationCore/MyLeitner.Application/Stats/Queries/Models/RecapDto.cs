namespace MyLeitner.Application.Stats.Queries.Models;

/// <summary>
/// «امروز چطور بود» در پایان مرور، برای همهٔ جعبه‌ها. میانگین‌ها فقط روزهای فعال را می‌شمرند و تعداد روزهای فعال جدا می‌آید.
/// </summary>
/// <param name="Last7">۷ روز اخیر، قدیمی‌ترین اول، امروز آخر (برای نمودار کوچک).</param>
/// <param name="Week">۷ روز قبل از امروز (بدون امروز).</param>
/// <param name="Month">۳۰ روز قبل از امروز (بدون امروز).</param>
/// <param name="BacklogClearedWeek">کارت‌های عقب‌افتاده (موعد قبل از روز جواب) که در ۷ روز اخیر، با امروز، جواب گرفتند.</param>
/// <param name="IsMonthRecord">امروز بیشتر از هر روزِ ۳۰ روز قبل جواب داده شده.</param>
/// <param name="Milestone">اگر امروز از یک عدد رند گذشتی (۱۰۰، ۵۰۰، ۱۰۰۰، ۲۰۰۰، ...): همان عدد.</param>
public record RecapDto(
    DateOnly Today,
    RecapTodayDto TodayStats,
    int Yesterday,
    List<DayCountDto> Last7,
    RecapPeriodDto Week,
    RecapPeriodDto Month,
    int Streak,
    int BestStreak,
    int BacklogClearedWeek,
    int BacklogRemaining,
    bool IsMonthRecord,
    int? Milestone,
    int DueTomorrow);

/// <param name="Seconds">جمع زمان پاسخ‌ها.</param>
/// <param name="QualityPercent">میانگین کیفیت جواب‌ها (AnswerQuality: مطمئن ۱، مرور ۰٫۷۵، شک ۰٫۵، شانسی/اشتباه ۰)؛ null بدون جواب.</param>
/// <param name="StepsUp">جواب‌هایی که کارت را حداقل یک گام جلو بردند.</param>
/// <param name="Mastered">کارت‌هایی که امروز یادگرفته‌شده شدند (با «بلدمش» یا رسیدن به سقف).</param>
public record RecapTodayDto(int Count, int Seconds, double? QualityPercent, int StepsUp, int Mastered);

/// <param name="Days">طول بازه (۷ یا ۳۰).</param>
/// <param name="ActiveDays">روزهایی از بازه که حداقل یک جواب داشتند.</param>
/// <param name="AveragePerActiveDay">میانگین جواب در روزهای فعال؛ null بدون روز فعال.</param>
public record RecapPeriodDto(int Days, int ActiveDays, double? AveragePerActiveDay, double? QualityPercent);
