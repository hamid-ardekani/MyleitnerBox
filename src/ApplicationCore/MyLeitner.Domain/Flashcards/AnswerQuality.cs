namespace MyLeitner.Domain.Flashcards;

/// <summary>
/// کیفیت یک پاسخ از ۰ تا ۱؛ تنها تعریف «خوب جواب دادن» در برنامه: مطمئن ۱، مرور ۰٫۷۵، شک ۰٫۵، شانسی ۰ (حدس زدن بلد بودن نیست)، اشتباه ۰.
/// همان وزن‌هایی که الگوریتم زمان‌بندی به پاسخ‌های مطمئن/مرور/شک می‌دهد.
/// </summary>
public static class AnswerQuality
{
    public static double Score(AnswerTypeEnum answer) => Score(answer.Id);

    public static double Score(short answerTypeId) => answerTypeId switch
    {
        0 => 1.0,  // Confident
        1 => 0.75, // NeedMoreReview
        2 => 0.5,  // NotSure
        _ => 0.0,  // UnCertain (lucky guess), Wrong
    };
}
