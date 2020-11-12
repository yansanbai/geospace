using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuboidLengthCondition : CuboidCondition
{
    public float length;

    public CuboidLengthCondition(float length)
    {
        this.length = length;

        GizmoLength gizmoLength = new GizmoLength(Cuboid.PNP_A, Cuboid.PNN_D);
        gizmos = new Gizmo[] { gizmoLength };
    }
}


public class CuboidLengthConditionTool : CuboidConditionTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormText("长");
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
        CuboidLengthCondition condition = new CuboidLengthCondition(formNum.num);

        return condition;
    }
}

public class CuboidLengthConditionState : ConditionState
{
    new CuboidLengthCondition condition;
    Cuboid geometry;

    public CuboidLengthConditionState(Tool tool, Condition condition, Geometry geometry): base(tool, condition)
    {
        if (condition is CuboidLengthCondition)
            this.condition = (CuboidLengthCondition)condition;

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

        formInput.inputs[0] = new FormText("长");
        formInput.inputs[1] = new FormText("=");
        formInput.inputs[2] = new FormNum(condition.length);

        return formInput;
    }
}