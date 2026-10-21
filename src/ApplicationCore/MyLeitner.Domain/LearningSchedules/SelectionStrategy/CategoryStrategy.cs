namespace MyLeitner.Domain.LearningSchedules.SelectionStrategy;

public class CategoryStrategy
{
    private const double DiversityPenalty = 0.3;

    public long? GetSelectionCriteria(CategorySelectionStrategy categorySelectionStrategy)
    {
        foreach (var card in categorySelectionStrategy.Boxes.SelectMany(box => box.Cards))
            card.EffectivePriority = card.basePriority - categorySelectionStrategy.BoxRepeatCount[card.boxId] * DiversityPenalty;

        var effectivePriorityCard = categorySelectionStrategy
            .Boxes
            .SelectMany(box => box.Cards)
            .MaxBy(c => c.EffectivePriority);

        if (effectivePriorityCard == null)
            return null;

        categorySelectionStrategy.BoxRepeatCount[effectivePriorityCard.boxId] += 1;
        return effectivePriorityCard.FlashId;
    }
}

public record CategorySelectionStrategy(Dictionary<long, int> BoxRepeatCount, List<BoxSelectionStrategy> Boxes);

public record BoxSelectionStrategy(List<FlashcardSelectionStrategy> Cards);

public record FlashcardSelectionStrategy(long FlashId, long boxId, double basePriority)
{
    public double EffectivePriority { get; set; }
}