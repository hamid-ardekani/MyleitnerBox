using System;
using System.Collections.Generic;

namespace MyLeitner.Persistence.ReadModel.Sql.ReadModels;

public partial class Flashcard
{
    public long Id { get; set; }

    public short Order { get; set; }

    public string Front { get; set; } = null!;

    public string Back { get; set; } = null!;

    public string? FrontSearchText { get; set; }

    public string? BackSearchText { get; set; }

    public string Tags { get; set; } = null!;

    public DateTime? UpdateDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long BoxId { get; set; }

    public virtual Box Box { get; set; } = null!;

    public virtual LearningSchedule? LearningSchedule { get; set; }
}
