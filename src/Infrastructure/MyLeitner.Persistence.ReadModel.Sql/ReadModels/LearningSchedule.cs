using System;
using System.Collections.Generic;

namespace MyLeitner.Persistence.ReadModel.Sql.ReadModels;

public partial class LearningSchedule
{
    public long Id { get; set; }

    public long FlashcardId { get; set; }

    public long UserId { get; set; }

    public DateTime? LastReviewDate { get; set; }

    public DateTime? LastAnswerDate { get; set; }

    public DateTime? NextAnswerDate { get; set; }

    public double Stability { get; set; }

    public double Difficulty { get; set; }

    public double Interval { get; set; }

    public double Priority { get; set; }

    /// <summary>MyLeitner.Domain.LearningSchedules.LearningStatus - فقط Learning (0) در مرور می‌آید.</summary>
    public int Status { get; set; }

    public DateTime? StatusChangedAt { get; set; }

    /// <summary>کاربر دستی کارت جدید را برای همین روز به مرور اضافه کرده (فقط همان روز اثر دارد).</summary>
    public DateTime? AddedToReviewOn { get; set; }

    public virtual Flashcard Flashcard { get; set; } = null!;
}
