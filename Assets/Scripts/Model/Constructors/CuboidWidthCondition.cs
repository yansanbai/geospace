using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuboidWidthCondition : CuboidCondition
{
    public float width;

    public CuboidWidthCondition(float width)
    {
        this.width = width;

        GizmoLength gizmoLength = new GizmoLength(Cuboid.PNP_A, Cuboid.NNP_B);
        gizmos = new Gizmo[] { gizmoLength };
    }
}

public class CuboidWidthConditionTool : CuboidConditionTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormText("宽");
        formInput.inputs[1] = new FormText("=");
        formInput.inputs[2] = new FormNum();

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        if (!(geometry is Cuboid))
            return false;
        Cuboid cuboid = (Cuboid)geometry;

        FormNum formNum = (FormNum)formInput.inputs[2];
        if (!IsValidLength(formNum))
            return false;

        return true;
    }

    public override Condition GenerateCondition(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        FormNum formNum = (FormNum)formInput.inputs[2];
        CuboidWidthCondition condition = new CuboidWidthCondition(formNum.num);

        return condition;
    }
}

public class CuboidWidthConditionState : ConditionState
{
    new CuboidWidthCondition condition;
    Cuboid geometry;

    public CuboidWidthConditionState(Tool tool, Condition condition, Geometry geometry): base(tool, condition)
    {
        if (condition is CuboidWidthCondition)
            this.condition = (CuboidWidthCondition)condition;

        if (geometry is Cuboid)
            this.geometry = (Cuboid)geometry;
    }

    public override int[] DependVertices()
    {
        return new int[] { };
    }

    public override FormInput Title()
    {
        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormText("宽");
        formInput.inputs[1] = new FormText("=");
        formInput.inputs[2] = new FormNum(condition.width);

        return formInput;
    }

}