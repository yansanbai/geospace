using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCondition : FunctionCondition
{
    public string fomula;

    public FreeCondition(string fomula,int index)
    {
        this.fomula = fomula;
        this.index = index;

    }
}


public class FreeConditionTool : FunctionConditionTool
{
    Function func;
    public override FormInput FormInput()
    {
        return null;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        if (!(geometry is Function))
            return false;
        func = (Function)geometry;
        return true;
    }

    public override Condition GenerateCondition(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        FreeCondition condition = new FreeCondition(func.Getfomula(),func.Getindex());

        return condition;
    }
}

public class FreeConditionState : ConditionState
{
    new FreeCondition condition;
    Function geometry;

    public FreeConditionState(Tool tool, Condition condition, Geometry geometry) : base(tool, condition)
    {
        if (condition is FreeCondition)
            this.condition = (FreeCondition)condition;

        if (geometry is Function)
            this.geometry = (Function)geometry;
    }

    public override int[] DependVertices()
    {
        return new int[] { };
    }

    public override FormInput Title()
    {
        FormInput formInput = new FormInput(1);

        formInput.inputs[0] = new FormText(condition.fomula);

        return formInput;
    }

}
