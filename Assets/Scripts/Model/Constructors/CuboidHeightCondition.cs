using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuboidHeightCondition : CuboidCondition
{
    public float height;

    public CuboidHeightCondition(float height)
    {
        this.height = height;

        GizmoLength gizmoLength = new GizmoLength(Cuboid.PNP_A, Cuboid.PPP_E);
        gizmos = new Gizmo[] { gizmoLength };
    }
}


public class CuboidHeightConditionTool : CuboidConditionTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormText("高");
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
        CuboidHeightCondition condition = new CuboidHeightCondition(formNum.num);

        return condition;
    }
}

public class CuboidHeightConditionState : ConditionState
{
    new CuboidHeightCondition condition;
    Cuboid geometry;

    public CuboidHeightConditionState(Tool tool, Condition condition, Geometry geometry) : base(tool, condition)
    {
        if (condition is CuboidHeightCondition)
            this.condition = (CuboidHeightCondition)condition;

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

        formInput.inputs[0] = new FormText("高");
        formInput.inputs[1] = new FormText("=");
        formInput.inputs[2] = new FormNum(condition.height);

        return formInput;
    }

}