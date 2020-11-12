using UnityEngine;


public abstract class Condition
{
    public Gizmo[] gizmos;
}

public abstract class ConditionTool
{
    public abstract FormInput FormInput();

    public abstract bool ValidateInput(Geometry geometry, FormInput formInput);

    public abstract Condition GenerateCondition(Geometry geometry, FormInput formInput);

    protected bool IsValidLength(FormNum formNum)
    {
        bool result = formNum.num > 0 && formNum.num < 10;
        return result;
    }

    protected bool IsValidAngle(FormNum formNum)
    {
        bool result = formNum.num > 0 && formNum.num < 180;
        return result;
    }
}

public abstract class ConditionState : State
{
    public Condition condition;

    public ConditionState(Tool tool) : base(tool)
    {

    }

    public ConditionState(Tool tool, Condition condition) : base(tool)
    {
        this.condition = condition;
    }
}