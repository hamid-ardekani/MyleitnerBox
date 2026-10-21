namespace MyLeitner.Persistence.ReadModel.Sql.ReadModels;

public partial class ReviewLog
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long FlashcardId { get; set; }

    public long BoxId { get; set; }

    public long? CategoryId { get; set; }

    public short AnswerTypeId { get; set; }

    public int ResponseTimeInSeconds { get; set; }

    public DateTime AnsweredAt { get; set; }

    public DateOnly ReviewDate { get; set; }

    public DateTime? PreviousAnsweredAt { get; set; }

    public DateTime? DueAt { get; set; }

    public double StabilityBefore { get; set; }

    public double DifficultyBefore { get; set; }

    public double IntervalBefore { get; set; }

    public double StabilityAfter { get; set; }

    public double DifficultyAfter { get; set; }

    public double IntervalAfter { get; set; }
}
