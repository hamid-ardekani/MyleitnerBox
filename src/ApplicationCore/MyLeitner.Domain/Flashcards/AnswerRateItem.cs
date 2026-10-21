namespace MyLeitner.Domain.Flashcards;

/// <summary>زمان پاسخ را فقط ثبت کن و نمایش نده؛ اگر کاربر بداند عجله می‌کند.</summary>
public class AnswerRateItem
{
    private int _responseTimeInSeconds;
    private double _ratio;
    private int _medianExpectedTime  = 60;

    public int Process(int responseTimeInSeconds, string? timesForCorrectAnswer, int userGlobalMedian)
    {
        var responseTimesForCorrectAnswer = string.IsNullOrWhiteSpace(timesForCorrectAnswer)
            ? new List<int>()
            : timesForCorrectAnswer.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

        _responseTimeInSeconds = responseTimeInSeconds;
        SetMedianTime(responseTimesForCorrectAnswer, userGlobalMedian);
        SetRatio();
        return SetTimePenalty();
    }

    /// <summary>متوسط زمان پاسخ برای این فلش‌کارت</summary>
    /// <param name="responseTimesForCorrectAnswer"></param>
    /// <param name="userGlobalMedian"></param>
    private void SetMedianTime(List<int> responseTimesForCorrectAnswer, int userGlobalMedian)
    {
        if (responseTimesForCorrectAnswer.Count < 5)
        {
            _medianExpectedTime = userGlobalMedian > 0 ? userGlobalMedian : 60;
            return;
        }

        var orderList = responseTimesForCorrectAnswer.OrderBy(c => c).ToList();
        var middle = orderList.Count / 2;

        _medianExpectedTime = orderList.Count % 2 == 1
            ? orderList[middle]
            : (orderList[middle - 1] + orderList[middle]) / 2;
    }


    private void SetRatio()
    {
        _ratio = _medianExpectedTime > 0 ? (double)_responseTimeInSeconds / _medianExpectedTime : 1.0;
    }

    private int SetTimePenalty()
    {
        if (_ratio < 0.7)
            return 5; //fast

        if (_ratio < 1.0)
            return 4;

        if (_ratio < 1.3)
            return 3;

        return 2; //slow
    }
}