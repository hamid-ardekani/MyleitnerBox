using MyLeitner.Application.Contract.Dtos;

namespace MyLeitner.Application.Stats.Queries.Models;

/// <param name="Today">امروزِ محلی (تهران) از دید سرور - کلاینت روزهای آینده را با آن تشخیص می‌دهد.</param>
public record StatsDto(DateOnly Today, StatsSummaryDto Summary, StatsPeriodDto Period, List<DayCountDto> Forecast, List<BoxStepsDto> Boxes);

/// <summary>
/// کارت‌های بالای داشبورد - مستقل از بازهٔ انتخاب‌شده.
/// </summary>
/// <param name="DueReviewsToday">مرورهایی که امروز واقعاً در صف‌اند: همهٔ سرِ موعدهای امروز + فقط سهم آزادشدهٔ بدهی (BacklogQuota).</param>
/// <param name="NewCardsToday">کارت‌های جدیدی که امروز وارد مرور می‌شوند - با سقف روزانهٔ هر جعبه، نه همهٔ کارت‌های جدید.</param>
/// <param name="StreakDays">روزهای پشت‌سرهم با حداقل یک پاسخ، تا امروز (اگر امروز هنوز مروری نشده، تا دیروز).</param>
/// <param name="DueTomorrow">کارت‌هایی که موعدشان دقیقاً فردا (محلی) است.</param>
/// <param name="BacklogRemaining">کل کارت‌های عقب‌افتاده (موعد قبل از امروز) که هنوز مانده‌اند - فقط سهمی از آن‌ها امروز در صف است.</param>
public record StatsSummaryDto(int ReviewedToday, int DueReviewsToday, int NewCardsToday, int StreakDays, int DueTomorrow, int BacklogRemaining = 0);

/// <param name="From">اولین روز بازه (محلی، شامل).</param>
/// <param name="To">آخرین روز بازه (محلی، شامل) - ممکن است در آینده باشد، مثلاً بقیهٔ همین هفته.</param>
/// <param name="CorrectPercent">درصد پاسخ‌های مطمئن/مرور/شک از کل؛ null وقتی پاسخی نیست.</param>
/// <param name="CardsAdded">فقط کارت‌هایی که تاریخ ساخت دارند (ساخته‌شده بعد از اضافه‌شدن ستون CreatedAt).</param>
/// <param name="Buckets">هفته/ماه: یک ستون برای هر روز؛ سال: یک ستون برای هر ماه شمسی؛ امروز: خالی.</param>
/// <param name="Days">فقط برای سال: تعداد پاسخ هر روزِ سال (برای نقشهٔ حرارتی)؛ بقیهٔ بازه‌ها خالی.</param>
public record StatsPeriodDto(
    DateOnly From,
    DateOnly To,
    int TotalAnswers,
    int DistinctCards,
    List<AnswerTypeCountDto> AnswerTypes,
    double? CorrectPercent,
    double? AverageResponseSeconds,
    int ActiveDays,
    int CardsAdded,
    List<StatsBucketDto> Buckets,
    List<DayCountDto> Days);

public record AnswerTypeCountDto(short AnswerTypeId, int Count);

public record StatsBucketDto(DateOnly From, DateOnly To, int Count);

public record DayCountDto(DateOnly Date, int Count);

/// <summary>وضعیت همهٔ کارت‌های یک جعبه (نه فقط امروز).</summary>
/// <param name="Steps">تعداد کارت‌های در حال یادگیری در هر گام؛ اندیس ۰ = جدید، ۱..۸ = گام‌ها.</param>
/// <param name="NewCards">سهم کارت جدید امروز و دلیلش (پاداش دقت / بار مرور).</param>
public record BoxStepsDto(long BoxId, string Name, List<int> Steps, int Mastered, int Archived, NewCardQuotaDto? NewCards = null, BacklogQuotaDto? Backlog = null);
