namespace MyLeitner.Application.Stats.Queries.Models;

/// <summary>
/// «پیشرفت من»: امروز/هفته/ماه در برابر هدف و گذشتهٔ خود کاربر. مقایسهٔ هفته و ماه <b>تا همان نقطه</b> است
/// (سومین روز هفته با سه روز هفتهٔ قبل). شمارش = تعداد جواب‌ها.
/// </summary>
/// <param name="Goals">هدف‌های کاربر؛ بدون ذخیره، مقدار پیش‌فرض (۱۰ / ۱۰۰ / ۴۰۰).</param>
public record ProgressDto(DateOnly Today, ReviewGoalsDto Goals, ProgressDayDto Day, ProgressSpanDto Week, ProgressSpanDto Month, ProgressAveragesDto Averages);

/// <summary>
/// میانگین جواب در روز، فقط روزهای فعال و بدون امروز؛ null = داده‌ای نیست. هفته = ۷ روز اخیر، ماه = ۳۰ روز اخیر،
/// سال = سال شمسی تا دیروز (فقط با حداقل ۳۰ روز تاریخچه).
/// </summary>
public record ProgressAveragesDto(double? Week, double? Month, double? Year);

public record ReviewGoalsDto(int Daily, int Weekly, int Monthly);

/// <param name="SameDayLastWeek">همین روز هفته در هفتهٔ قبل (هفت روز پیش).</param>
/// <param name="BestDay">رکورد روزانهٔ کاربر: بیشترین جواب در یک روز، از همهٔ روزهای قبل از امروز (۰ = هنوز روزی نبوده).</param>
/// <param name="SameDateLastMonth">همین تاریخ (شمارهٔ روز ماه) در ماه شمسی قبل؛ ۰ اگر آن ماه این روز را نداشت یا داده‌ای نبود.</param>
public record ProgressDayDto(int Today, int Yesterday, int DayBefore, int SameDayLastWeek, int BestDay, int SameDateLastMonth);

/// <param name="Count">جواب‌های این بازه از اولش تا امروز.</param>
/// <param name="PreviousSamePoint">بازهٔ قبلی (هفتهٔ قبل / ماه شمسی قبل) از اولش تا همین شمارهٔ روز.</param>
/// <param name="PreviousTotal">کل بازهٔ قبلی.</param>
/// <param name="DayNumber">امروز چندمین روز بازه است (هفته ۱..۷، ماه ۱..۳۱).</param>
/// <param name="TotalDays">طول بازهٔ جاری (۷ یا ۲۹..۳۱).</param>
/// <param name="BestTotal">رکورد: بیشترین مجموع یک هفته/ماهِ تمام‌شدهٔ قبلی (۰ = هنوز نبوده).</param>
public record ProgressSpanDto(int Count, int PreviousSamePoint, int PreviousTotal, int DayNumber, int TotalDays, int BestTotal);
