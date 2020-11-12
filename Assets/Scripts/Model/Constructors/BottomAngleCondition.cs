using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BottomAngleCondition : TriPyramidBottomCondition
{
    public CornerRefer corner;
    public float angle;

    public BottomAngleCondition(int id1, int id2, int id3, float angle) : base(2)
    {
        this.corner = new CornerRefer(id1, id2, id3);
        this.angle = angle;

        GizmoCorner gizmoCorner = new GizmoCorner(corner);
        GizmoAngle gizmoAngle = new GizmoAngle(corner);
        gizmos = new Gizmo[] { gizmoCorner, gizmoAngle };
    }
}

public class BottomAngleConditionTool : TriPyramidConditionTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(4);

        formInput.inputs[0] = new FormText("∠");
        formInput.inputs[1] = new FormElement(3);
        formInput.inputs[2] = new FormText("=");
        formInput.inputs[3] = new FormNum();

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        if (!(geometry is TriPyramid))
            return false;
        TriPyramid triPyramid = (TriPyramid)geometry;

        FormElement formElement = (FormElement)formInput.inputs[1];
        if (!IsBottomCorner(triPyramid, formElement))
            return false;

        FormNum formNum = (FormNum)formInput.inputs[3];
        if (!IsValidAngle(formNum))
            return false;

        return true;
    }

    public override Condition GenerateCondition(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        FormElement formElement = (FormElement)formInput.inputs[1];
        string[] fields = formElement.fields;
        int id1 = geometry.SignVertex(fields[0]);
        int id2 = geometry.SignVertex(fields[1]);
        int id3 = geometry.SignVertex(fields[2]);
        FormNum formNum = (FormNum)formInput.inputs[3];
        BottomAngleCondition condition = new BottomAngleCondition(id1, id2, id3, formNum.num);

        return condition;
    }
}

public class BottomAngleConditionState : ConditionState
{
    new BottomAngleCondition condition;
    TriPyramid geometry;

    public BottomAngleConditionState(Tool tool, Condition condition, Geometry geometry) : base(tool, condition)
    {
        if (condition is BottomAngleCondition)
            this.condition = (BottomAngleCondition)condition;

        if (geometry is TriPyramid)
            this.geometry = (TriPyramid)geometry;
    }

    public override int[] DependVertices()
    {
        int[] ids = new int[] {
            condition.corner.id1,
            condition.corner.id2,
            condition.corner.id3,
        };
        return ids;
    }

    public override FormInput Title()
    {
        FormElement formElement = new FormElement(3);
        formElement.fields[0] = geometry.VertexSign(condition.corner.id1);
        formElement.fields[1] = geometry.VertexSign(condition.corner.id2);
        formElement.fields[2] = geometry.VertexSign(condition.corner.id3);

        FormInput formInput = new FormInput(5);

        formInput.inputs[0] = new FormText("∠");
        formInput.inputs[1] = formElement;
        formInput.inputs[2] = new FormText("=");
        formInput.inputs[3] = new FormNum(condition.angle);
        formInput.inputs[4] = new FormText("°");

        return formInput;
    }

}