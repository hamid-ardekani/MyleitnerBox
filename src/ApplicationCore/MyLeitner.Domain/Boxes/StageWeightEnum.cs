using Dela.Toolkit.Domain.Enumerations;

namespace MyLeitner.Domain.Boxes;

/// <summary>مراحل یادگیری؛ Weak و New مهم‌ترند و Mastered تقریباً حذف می‌شود.</summary>
public class StageWeightEnum : Enumeration
{
    public static readonly StageWeightEnum New = new(0, nameof(New));
    public static readonly StageWeightEnum Learning = new(1, nameof(Learning));
    public static readonly StageWeightEnum SuperWeak = new(2, nameof(SuperWeak));
    public static readonly StageWeightEnum Weak = new(3, nameof(Weak));
    public static readonly StageWeightEnum Medium = new(5, nameof(Medium));
    public static readonly StageWeightEnum Strong = new(6, nameof(Strong));
    public static readonly StageWeightEnum Mastered = new(7, nameof(Mastered));
    public static readonly StageWeightEnum SuperMastered = new(8, nameof(SuperMastered));
    public static readonly StageWeightEnum Answered = new(100, nameof(Answered));

    protected StageWeightEnum()
    {
    }

    protected StageWeightEnum(short id, string title) : base(id, title)
    {
    }

    public static StageWeightEnum Create(short id)
    {
        return List().First(c => c.Id == id);
    }

    private static IEnumerable<StageWeightEnum> List() => [New, Learning, SuperWeak, Weak, Strong, Mastered, SuperMastered, Answered];

    public static StageWeightEnum FromId(short id)
    {
        return id switch
        {
            0 => New,
            1 => Learning,
            2 => SuperWeak,
            3 => Weak,
            4 => Medium,
            5 => Strong,
            6 => Mastered,
            7 => SuperMastered, 
            _ => Answered
        }; 
    }
}