using System;
using System.Collections.Generic;

namespace MyLeitner.Persistence.ReadModel.Sql.ReadModels;

public partial class Box
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public short OrderBox { get; set; }

    public DateTime? StartDate { get; set; }

    public bool ReadFlashcardAloud { get; set; }

    public short DailyNewCardLimit { get; set; }

    public short DailyBacklogFloor { get; set; }

    public long UserId { get; set; }

    public DateOnly? BacklogBonusDate { get; set; }

    public short BacklogBonus { get; set; }

    public virtual ICollection<CategoryBox> CategoryBoxes { get; set; } = new List<CategoryBox>();

    public virtual ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
}
