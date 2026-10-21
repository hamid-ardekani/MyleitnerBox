using Dela.Toolkit.Domain;
using Dela.Toolkit.Domain.Enumerations;
using MyLeitner.Domain.Boxes;
using MyLeitner.Domain.Boxes.Exceptions;

namespace MyLeitner.Domain.Common;

public class StageWeightFactor : ValueObject
{
    public StageWeightEnum Value { get; private set; }

    public StageWeightFactor()
    {
    }

    protected StageWeightFactor(StageWeightEnum item) : this()
    {
        Value = item ?? throw new StageWeightNullException();
    }

    public static StageWeightFactor Create(StageWeightEnum item)
    {
        return new StageWeightFactor(item);
    }

    public static StageWeightFactor Create(int id)
    {
        if (id < 0)
            id = 100;
        return new StageWeightFactor(Enumeration.FromValue<StageWeightEnum>(id));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public StageWeightEnum ComputeStageWeight(double stability, double difficulty)
    {
        if (stability > 10 && difficulty > 100)
            return StageWeightEnum.SuperWeak;

        if (stability > 20 && difficulty > 90)
            return StageWeightEnum.Weak;

        if (stability > 30 && difficulty > 80)
            return StageWeightEnum.Medium;

        if (stability > 50 && difficulty > 50)
            return StageWeightEnum.Strong;

        if (stability > 70 && difficulty > 30)
            return StageWeightEnum.Mastered;

        if (stability > 90 && difficulty > 10)
            return StageWeightEnum.Answered;

        return StageWeightEnum.Learning;
    }

    public static short GetPower(StageWeightEnum stageWeight)
    {
        if (stageWeight.Equals(StageWeightEnum.Answered))
            return 100;

        if (stageWeight.Equals(StageWeightEnum.SuperMastered))
            return 8;
        if (stageWeight.Equals(StageWeightEnum.Mastered))
            return 7;
        if (stageWeight.Equals(StageWeightEnum.Strong))
            return 6;
        if (stageWeight.Equals(StageWeightEnum.Medium))
            return 5;
        if (stageWeight.Equals(StageWeightEnum.Weak))
            return 4;
        if (stageWeight.Equals(StageWeightEnum.SuperWeak))
            return 3;
        if (stageWeight.Equals(StageWeightEnum.Learning))
            return 2;
        if (stageWeight.Equals(StageWeightEnum.New))
            return 1;

        return 0;
    }
}