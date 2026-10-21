namespace MyLeitner.Application.Learnings.Enums;

public enum AnswerRateTimeType
{
    Confident = 0,
    /// <summary>پاسخ را بلد بوده ولی مرور لازم دارد</summary>
    NeedMoreReview = 1,
    /// <summary>پاسخ را بلد بوده ولی مطمئن نبوده</summary>
    NotSure = 2,
    /// <summary>پاسخ درست بوده ولی شانسی</summary>
    UnCertain = 3,
    /// <summary>پاسخ اشتباه</summary>
    Wrong = 4, 
}