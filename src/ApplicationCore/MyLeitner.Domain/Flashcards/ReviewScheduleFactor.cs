namespace MyLeitner.Domain.Flashcards;

/// <summary>فاصلهٔ مرور بعدی بر حسب دقیقه</summary>
public class ReviewScheduleFactor
{
    private const int FirstReviewSchedule = 2; // 2 min
    private const int SecondReviewSchedule = 5; // 5 min
    private const int ThirdReviewSchedule = 10; // 10 min
    private const int ForthReviewSchedule = 20; // 20 min
    private const int FifthReviewSchedule = 30; // 30 min
    private const int SixthReviewSchedule = 60; // 1 hour
    private const int SeventhReviewSchedule = 120; // 2 hour
    private const int EighthReviewSchedule = 180; // 3 hour
    private const int NinthReviewSchedule = 240; // 4 hour
    private const int MaxReviewSchedule = 360; // 4 hour

    public short? Value { get; private set; }

    private ReviewScheduleFactor(short value)
    {
        Value = value;
    }

    public static ReviewScheduleFactor Create(short? value)
    {
        return new ReviewScheduleFactor(value ?? 0);
    }


    public void SetReviewScheduleAfter(AnswerTypeEnum answerRateTimeType, short? reviewTime, int answerQuality)
    {
        if (answerRateTimeType.Id == AnswerTypeEnum.Confident.Id)
        {
            Value = null;
            return;
        }

        Value = reviewTime switch
        {
            -4 => ActualInterval(FirstReviewSchedule, answerQuality),
            -3 => ActualInterval(SecondReviewSchedule, answerQuality),
            -2 => ActualInterval(ThirdReviewSchedule, answerQuality),
            -1 => ActualInterval(ForthReviewSchedule, answerQuality),
            0 => ActualInterval(FifthReviewSchedule, answerQuality),
            1 => ActualInterval(SixthReviewSchedule, answerQuality),
            2 => ActualInterval(SeventhReviewSchedule, answerQuality),
            3 => ActualInterval(EighthReviewSchedule, answerQuality),
            4 => ActualInterval(NinthReviewSchedule, answerQuality),
            _ => MaxReviewSchedule,
        };
    }

    private static short ActualInterval(double interval, double difficulty)
    {
        var difficultyFactor = 1 + difficulty;
        var time = (int)(interval / difficultyFactor);

        return (short)Math.Clamp(time, FirstReviewSchedule, MaxReviewSchedule);
    }
}