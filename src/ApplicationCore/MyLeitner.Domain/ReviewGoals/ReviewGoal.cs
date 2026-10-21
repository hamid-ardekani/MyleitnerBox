using Dela.Toolkit.Domain;
using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.ReviewGoals;

/// <summary>
/// هدف‌های مرور هر کاربر: چند جواب در روز، هفته (شنبه تا جمعه) و ماه (شمسی). جواب‌ها از <see cref="ReviewLogs.ReviewLog"/> شمرده می‌شوند
/// و این aggregate فقط عددهای هدف را نگه می‌دارد؛ بدون ذخیره، مقدار پیش‌فرض اعمال می‌شود.
/// </summary>
public class ReviewGoal : AggregateRoot<long>
{
    public const int DefaultDaily = 10;
    public const int DefaultWeekly = 100;
    public const int DefaultMonthly = 400;

    public const int MinTarget = 1;
    public const int MaxTarget = 99999;

    public long UserId { get; private set; }
    public int Daily { get; private set; }
    public int Weekly { get; private set; }
    public int Monthly { get; private set; }

    public static ReviewGoal ForUser(long userId, int daily, int weekly, int monthly)
    {
        var goal = new ReviewGoal { UserId = userId };
        goal.Update(daily, weekly, monthly);
        return goal;
    }

    public void Update(int daily, int weekly, int monthly)
    {
        if (!IsValid(daily) || !IsValid(weekly) || !IsValid(monthly))
            throw new ReviewGoalUnValidException();

        Daily = daily;
        Weekly = weekly;
        Monthly = monthly;
    }

    private static bool IsValid(int target) => target is >= MinTarget and <= MaxTarget;
}

public class ReviewGoalUnValidException : ExceptionManager
{
    public ReviewGoalUnValidException() : base(Errors.ReviewGoalUnValidException)
    {
    }
}
