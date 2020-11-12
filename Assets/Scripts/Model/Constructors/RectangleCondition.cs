using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectangleCondition : ResolvedBodyCondition
{
    public float height;

    public float width;

    public RectangleCondition(float height, float width)
    {
        this.height = height;
        this.width = width;

    }
}


public class RectangleConditionTool : ResolvedBodyConditionTool
{
    public override FormInput FormInput()
    {
        // FormInput formInput = new FormInput(7);

        // formInput.inputs[0] = new FormText("垂直边长");
        // formInput.inputs[1] = new FormText("=");
        // formInput.inputs[2] = new FormNum();
        // formInput.inputs[3] = new FormText("，");
        // formInput.inputs[4] = new FormText("水平边长");
        // formInput.inputs[5] = new FormText("=");
        // formInput.inputs[6] = new FormNum();
        return null;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        if (!(geometry is ResolvedBody))
            return false;
        ResolvedBody resolvedBody = (ResolvedBody)geometry;

        // FormNum height = (FormNum)formInput.inputs[2];
        // FormNum width = (FormNum)formInput.inputs[6];
        // if (!IsValidLength(height) || !IsValidLength(width))
        //     return false;

        return true;
    }

    public override Condition GenerateCondition(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        // FormNum height = (FormNum)formInput.inputs[2];
        // FormNum width = (FormNum)formInput.inputs[6];
        RectangleCondition condition = new RectangleCondition(2, 1);

        return condition;
    }
}

public class RectangleConditionState : ConditionState
{
    new RectangleCondition condition;
    ResolvedBody geometry;

    public RectangleConditionState(Tool tool, Condition condition, Geometry geometry) : base(tool, condition)
    {
        if (condition is RectangleCondition)
            this.condition = (RectangleCondition)condition;

        if (geometry is ResolvedBody)
            this.geometry = (ResolvedBody)geometry;
    }

    public override int[] DependVertices()
    {
        return new int[] { };
    }

    public override FormInput Title()
    {
        FormInput formInput = new FormInput(1);

        formInput.inputs[0] = new FormText("旋转图形: 四边形");

        return formInput;
    }

}