using Dela.Toolkit.Domain; 
using MyLeitner.Domain.Boxes.Exceptions;

namespace MyLeitner.Domain.Boxes;

public class Box : AggregateRoot<long> 
{
    public const short DefaultDailyNewCardLimit = 10;
    private const short MinDailyNewCardLimit = 1;
    private const short MaxDailyNewCardLimit = 99;

    /// <summary>بیش از این تعداد «اشتباه» در یک روز، جعبه آن روز کارت جدید نمی‌آورد (<see cref="NewCardQuota"/>).</summary>
    public const int MaxTodayWrongForNewCards = NewCardQuota.MaxTodayWrong;

    public string Name { get; private set; } = null!;
    public short OrderBox { get; private set; }

    public DateTime? StartDate { get; private set; }
    public bool ReadFlashcardAloud { get; private set; }

    /// <summary>سقف کارت‌های جدید (هیچ‌وقت پاسخ‌نگرفته) که این جعبه در یک روز وارد مرور می‌کند؛ کاربر تنظیم می‌کند.</summary>
    public short DailyNewCardLimit { get; private set; } = DefaultDailyNewCardLimit;

    /// <summary>کفِ روزانهٔ کارت‌های عقب‌افتاده (بدهی) که حتی در روز بد هم آزاد می‌شود (<see cref="BacklogQuota"/>).</summary>
    public short DailyBacklogFloor { get; private set; } = BacklogQuota.DefaultFloor;
    public long UserId { get; private set; }

    /// <summary>روزی که <see cref="BacklogBonus"/> برایش است (تاریخ محلی)؛ روزِ دیگر یعنی هدیه‌ای نیست.</summary>
    public DateOnly? BacklogBonusDate { get; private set; }

    /// <summary>کارت‌های عقب‌افتادهٔ اضافه‌ای که کاربر امروز در پایان مرور قبول کرده؛ روی کفِ روزانه اضافه می‌شود (<see cref="BacklogQuota"/>).</summary>
    public short BacklogBonus { get; private set; }

    /// <summary>هدیه‌ای که برای <paramref name="today"/> معتبر است؛ برای روزهای قبل صفر.</summary>
    public static int BacklogBonusFor(DateOnly? bonusDate, short bonus, DateOnly today) => bonusDate == today ? bonus : 0;

    public void AddBacklogBonus(DateOnly today, int amount)
    {
        var current = BacklogBonusFor(BacklogBonusDate, BacklogBonus, today);
        BacklogBonusDate = today;
        BacklogBonus = (short)Math.Min(BacklogQuota.DailyCap, current + Math.Max(0, amount));
    }

    public static Box BoxForAdd(string name, short orderBox, bool readFlashcardAloud, long userId, short? dailyNewCardLimit = null, short? dailyBacklogFloor = null)
    {
        var box = new Box
        {
            Name = name,
            ReadFlashcardAloud = readFlashcardAloud,
            UserId = userId,
        };

        box.UpdateOrder(orderBox);
        box.UpdateDailyNewCardLimit(dailyNewCardLimit ?? DefaultDailyNewCardLimit);
        box.UpdateDailyBacklogFloor(dailyBacklogFloor ?? BacklogQuota.DefaultFloor);
        return box;
    }

    /// <param name="dailyNewCardLimit">null = بدون تغییر (کلاینت قدیمی که این فیلد را نمی‌فرستد).</param>
    public void Update(string name, bool readFlashcardAloud, short? dailyNewCardLimit = null, short? dailyBacklogFloor = null)
    {
        Name = name;
        ReadFlashcardAloud = readFlashcardAloud;

        if (dailyNewCardLimit.HasValue)
            UpdateDailyNewCardLimit(dailyNewCardLimit.Value);

        if (dailyBacklogFloor.HasValue)
            UpdateDailyBacklogFloor(dailyBacklogFloor.Value);
    }

    public void UpdateDailyBacklogFloor(short dailyBacklogFloor)
    {
        if (dailyBacklogFloor is < BacklogQuota.MinFloor or > BacklogQuota.MaxFloor)
            throw new DailyBacklogFloorUnValidException();

        DailyBacklogFloor = dailyBacklogFloor;
    }

    public void UpdateDailyNewCardLimit(short dailyNewCardLimit)
    {
        if (dailyNewCardLimit is < MinDailyNewCardLimit or > MaxDailyNewCardLimit)
            throw new DailyNewFlashcardCountUnValidException();

        DailyNewCardLimit = dailyNewCardLimit;
    }

    public void UpdateOrder(short orderBox)
    {
        if (orderBox <= 0)
            throw new OrderUnValidException();

        OrderBox = orderBox;
    }


    /// <summary>
    /// سهم کارت جدید امروز این جعبه؛ قاعده در <see cref="NewCardQuota.Compute"/>.
    /// ایستا است تا سمت خواندن (ReadModel) هم بدون ساختن Aggregate از آن استفاده کند.
    /// </summary>
    public static NewCardQuota NewCardQuotaToday(
        short dailyNewCardLimit, int dueReviews, int todayWrong, int newCardsStartedToday, int recentReviews, int recentCorrectReviews) =>
        NewCardQuota.Compute(dailyNewCardLimit, dueReviews, todayWrong, newCardsStartedToday, recentReviews, recentCorrectReviews);
 
}