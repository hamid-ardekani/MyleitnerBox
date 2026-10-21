namespace MyLeitner.Persistence.ReadModel.Sql.ReadModels;

public partial class ReviewGoal
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public int Daily { get; set; }

    public int Weekly { get; set; }

    public int Monthly { get; set; }
}
