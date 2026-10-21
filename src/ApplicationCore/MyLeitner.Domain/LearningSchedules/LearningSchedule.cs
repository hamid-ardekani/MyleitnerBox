using Dela.Toolkit.Domain;
using MyLeitner.Domain.Flashcards;

namespace MyLeitner.Domain.LearningSchedules;

/// <summary>
/// آیا کارت هنوز در چرخهٔ مرور است. فقط <see cref="Learning"/> در مرور روزانه می‌آید.
/// </summary>
public enum LearningStatus
{
    Learning = 0,

    /// <summary>یادگرفته‌شده: در سقف فاصله (۱۸۰ روز) دوباره «مطمئن» جواب گرفت و خودکار از مرور خارج شد.</summary>
    Mastered = 1,

    /// <summary>کاربر دستی از مرور بیرونش گذاشت.</summary>
    Archived = 2
}

public class LearningSchedule : AggregateRoot<long>
{
    // نقطهٔ شروع اولین مرور یک فلش‌کارت تازه
    private const double InitialStability = 1;    // روز
    private const double InitialDifficulty = 0.3; // مقیاس 0..1

    // کف/سقف Stability بر حسب روز - کف تضمین می‌کند که هیچ کارتی همان روز دوباره نمایش داده نشود
    private const double MinStability = 1;
    public const double MaxStability = 180;

    private const double MinDifficulty = 0;
    private const double MaxDifficulty = 1;

    // جملهٔ کمکی مرورهای اول: به Stability مقدار (وزن اطمینان × این عدد ÷ Stability) روز اضافه می‌شود.
    // وقتی Stability کوچک است (کارت تازه) فاصلهٔ پاسخ‌ها را از هم باز می‌کند - وگرنه گردکردن به روز صحیح
    // همه را یکی می‌کند - و هرچه کارت پخته‌تر می‌شود محو می‌شود.
    private const double EarlyStepBonusDays = 3.5;

    // «شانسی» = بلد نبود و درست حدس زد: فاصله نصف (کارت یک گام عقب می‌رود) و سختی بیشتر - نه رشد.
    private const double GuessStabilityFactor = 0.5;
    private const double GuessDifficultyIncrease = 0.1;

    // «شک» = بلد بود ولی مطمئن نبود: فاصله تقریباً ثابت، فقط ۱۵٪ رشد (با ضریب سرعت پاسخ)؛ سختی دست نمی‌خورد.
    private const double NotSureGrowth = 1.15;

    public long FlashcardId { get; private set; }
    public long UserId { get; private set; }

    public DateTime? LastReviewDate { get; private set; }
    public DateTime? LastAnswerDate { get; private set; }
    public DateTime? NextAnswerDate { get; private set; }
    public double Stability { get; private set; }
    public double Difficulty { get; private set; }
    public double Interval { get; private set; }

    public LearningStatus Status { get; private set; } = LearningStatus.Learning;
    public DateTime? StatusChangedAt { get; private set; }

    /// <summary>
    /// کاربر دستی (از جستجو) گفته این کارتِ جدید «امروز» در مرور بیاید. فقط برای همان روز (محلی) اثر دارد: بدون انتظار جایزه/سهم
    /// وارد صف می‌شود و از سهم کارت جدید جعبه کم می‌شود؛ اگر جواب نگرفت، فردا دوباره «جدید» عادی است.
    /// </summary>
    public DateTime? AddedToReviewOn { get; private set; }

    /// <summary>میانهٔ زمان پاسخ‌دهی کاربر تا پیاده‌شدن محاسبهٔ واقعی؛ ثبت پاسخ و پیش‌نمایش هر دو از همین استفاده می‌کنند.</summary>
    public const int DefaultUserGlobalMedianSeconds = 60;

    /// <summary>
    /// در پایان <see cref="Interval"/> و <see cref="NextAnswerDate"/> به‌روزرسانی می‌شوند.
    /// </summary>
    /// <param name="answerRateTimeType">نوع پاسخ کاربر (از Confident تا Wrong).</param>
    /// <param name="responseTimeInSeconds">مدت‌زمانی که طول کشید تا کاربر پاسخ بدهد.</param>
    /// <param name="userGlobalMedian">میانهٔ زمان پاسخ‌دهی کاربر در کل کارت‌ها، برای مقایسهٔ سرعت این پاسخ.</param>
    public void AnswerProcess(AnswerTypeEnum answerRateTimeType, int responseTimeInSeconds, int userGlobalMedian)
    {
        var now = DateTime.UtcNow;
        var masters = WouldMaster(answerRateTimeType, responseTimeInSeconds, userGlobalMedian);
        var outcome = ComputeOutcome(answerRateTimeType, responseTimeInSeconds, userGlobalMedian);

        Stability = outcome.Stability;
        Difficulty = outcome.Difficulty;
        Interval = outcome.Interval;

        LastAnswerDate = now;
        LastReviewDate = now;
        NextAnswerDate = now.AddDays(Interval);

        if (masters)
            ChangeStatus(LearningStatus.Mastered, now);
    }

    /// <summary>
    /// نقطهٔ پایان یادگیری: جواب «مطمئن» که فاصلهٔ بعدی را به سقف (۱۸۰ روز) برساند، همان لحظه کارت را یادگرفته‌شده می‌کند.
    /// کارت گام ۸ که با «مطمئن» به سقف نمی‌رسد (مثلاً ۱۲۸ به ۱۵۰) در مرور می‌ماند. فقط «مطمئن» پایان می‌دهد.
    /// </summary>
    public bool WouldMaster(AnswerTypeEnum answerRateTimeType, int responseTimeInSeconds, int userGlobalMedian) =>
        Status == LearningStatus.Learning
        && answerRateTimeType.Equals(AnswerTypeEnum.Confident)
        && ComputeOutcome(answerRateTimeType, responseTimeInSeconds, userGlobalMedian).Stability >= MaxStability;

    /// <summary>
    /// «بلدمش» در مرور: کارت - جدید یا قدیمی - مستقیم یادگرفته‌شده می‌شود و از مرور بیرون می‌رود. پاسخ نیست: فاصله و موعد دست نمی‌خورد
    /// و در ReviewLogs ثبت نمی‌شود؛ زمانش در <see cref="StatusChangedAt"/> می‌ماند. با <see cref="RestoreToLearning"/> برمی‌گردد.
    /// </summary>
    public void MarkKnown()
    {
        if (Status != LearningStatus.Mastered)
            ChangeStatus(LearningStatus.Mastered, DateTime.UtcNow);
    }

    public void Archive()
    {
        if (Status != LearningStatus.Archived)
            ChangeStatus(LearningStatus.Archived, DateTime.UtcNow);
    }

    /// <summary>به مرور برمی‌گردد و همین امروز موعدش می‌رسد (کارت هیچ‌وقت پاسخ‌نگرفته جدید می‌ماند و سقف روزانه را رعایت می‌کند).</summary>
    public void RestoreToLearning()
    {
        if (Status == LearningStatus.Learning)
            return;

        var now = DateTime.UtcNow;
        if (NextAnswerDate > now)
            NextAnswerDate = now;

        ChangeStatus(LearningStatus.Learning, now);
    }

    /// <summary>
    /// کارت جدید (هیچ‌وقت پاسخ‌نگرفته و در مرور) را برای امروز به صف اضافه می‌کند. روی کارت پاسخ‌گرفته یا بیرون‌ازمرور اثری ندارد
    /// (آن‌ها با <see cref="RestoreToLearning"/> برمی‌گردند).
    /// </summary>
    public void AddToReview()
    {
        if (Status == LearningStatus.Learning && NextAnswerDate == null)
            AddedToReviewOn = DateTime.UtcNow;
    }

    private void ChangeStatus(LearningStatus status, DateTime now)
    {
        Status = status;
        StatusChangedAt = now;
    }

    /// <summary>
    /// فاصلهٔ مرور بعدی (به روز) اگر همین الان با این پاسخ و زمان داده شود، بدون تغییر دادن چیزی.
    /// همان محاسبهٔ <see cref="AnswerProcess"/> است، پس پیش‌نمایش دقیقاً همان چیزی است که ثبت می‌شود.
    /// </summary>
    public int PreviewIntervalDays(AnswerTypeEnum answerRateTimeType, int responseTimeInSeconds, int userGlobalMedian)
    {
        return (int)ComputeOutcome(answerRateTimeType, responseTimeInSeconds, userGlobalMedian).Interval;
    }

    private (double Stability, double Difficulty, double Interval) ComputeOutcome(
        AnswerTypeEnum answerRateTimeType, int responseTimeInSeconds, int userGlobalMedian)
    {
        var stability = Stability;
        var difficulty = Difficulty;

        if (stability <= 0)
        {
            stability = InitialStability;
            difficulty = InitialDifficulty;
        }

        var timeFactor = ComputeTimeFactor(responseTimeInSeconds, userGlobalMedian);

        if (answerRateTimeType.Equals(AnswerTypeEnum.Wrong))
        {
            // پاسخ غلط یعنی کارت هنوز جا نیفتاده: به کوتاه‌ترین فاصله برمی‌گردد و از نزدیک دوباره رشد می‌کند،
            // با Difficulty بالاتر تا رشد بعدی‌اش کندتر باشد.
            stability = MinStability;
            difficulty = Math.Min(MaxDifficulty, difficulty + 0.2);
        }
        else if (answerRateTimeType.Equals(AnswerTypeEnum.UnCertain))
        {
            stability = Math.Max(MinStability, stability * GuessStabilityFactor);
            difficulty = Math.Min(MaxDifficulty, difficulty + GuessDifficultyIncrease);
        }
        else if (answerRateTimeType.Equals(AnswerTypeEnum.NotSure))
        {
            var earlyStep = ConfidenceWeight(answerRateTimeType) * EarlyStepBonusDays / stability;
            stability = Math.Clamp(stability * NotSureGrowth * timeFactor + earlyStep, MinStability, MaxStability);
        }
        else
        {
            var confidence = ConfidenceWeight(answerRateTimeType);   // 0.25 .. 1.0
            var ease = 1.3 + (1 - difficulty) * 1.2;                 // هرچه سخت‌تر، رشد کمتر
            var growth = (1 + confidence * (ease - 1)) * timeFactor;
            var earlyStep = confidence * EarlyStepBonusDays / stability;

            stability = Math.Clamp(stability * growth + earlyStep, MinStability, MaxStability);
            difficulty = Math.Max(MinDifficulty, difficulty - 0.05 * confidence);
        }

        var interval = Math.Max(MinStability, Math.Round(stability));
        return (stability, difficulty, interval);
    }

    private static double ComputeTimeFactor(int responseTimeInSeconds, int userGlobalMedian)
    {
        var speedRating = new AnswerRateItem().Process(responseTimeInSeconds, null, userGlobalMedian);

        return speedRating switch
        {
            5 => 1.10, // پاسخ سریع => رشد کمی بیشتر
            4 => 1.00,
            3 => 0.95,
            _ => 0.85, // پاسخ کند => رشد کمتر
        };
    }

    private static double ConfidenceWeight(AnswerTypeEnum answerRateTimeType)
    {
        if (answerRateTimeType.Equals(AnswerTypeEnum.Confident)) return 1.0;
        if (answerRateTimeType.Equals(AnswerTypeEnum.NeedMoreReview)) return 0.75;
        return 0.5; // NotSure (فقط برای جملهٔ کمکی مرورهای اول)
    }

    public static LearningSchedule ForAdd(long userId, long boxId, long flashcardId)
    {
        return new LearningSchedule { FlashcardId = flashcardId, UserId = userId };
    }
}