using Dela.Toolkit.Domain.Enumerations;

namespace MyLeitner.Domain.Flashcards;

public class AnswerTypeEnum: Enumeration
{
    public static readonly AnswerTypeEnum Confident = new(0, nameof(Confident));
    public static readonly AnswerTypeEnum NeedMoreReview = new(1, nameof(NeedMoreReview));
    public static readonly AnswerTypeEnum NotSure = new(2, nameof(NotSure));
    public static readonly AnswerTypeEnum UnCertain = new(3, nameof(UnCertain));
    public static readonly AnswerTypeEnum Wrong = new(4, nameof(Wrong)); 
    
    
    protected AnswerTypeEnum()
    {
    }

    protected AnswerTypeEnum(short id, string title) : base(id, title)
    {
    }

    public static AnswerTypeEnum Create(short id)
    {
        return List().First(c => c.Id == id);
    }

    private static IEnumerable<AnswerTypeEnum> List() => [Confident, NeedMoreReview, NotSure, UnCertain, Wrong];

    public static AnswerTypeEnum FromId(short id)
    {
        return id switch
        {
            0 => Confident,
            1 => NeedMoreReview,
            2 => NotSure,
            3 => UnCertain,
            4 => Wrong 
        }; 
    }
}