using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TopLengthCondition : TriPyramidTopCondition
{
    public EdgeRefer edge;
    public float length;

    public TopLengthCondition(int id1, int id2, float length) : base(1)
    {
        this.edge = new EdgeRefer(id1, id2);
        this.length = length;

        GizmoLength gizmoLength = new GizmoLength(edge);
        gizmos = new Gizmo[] { gizmoLength };
    }
}

public class TopLengthConditionTool : TriPyramidConditionTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormElement(2);
        formInput.inputs[1] = new FormText("=");
        formInput.inputs[2] = new FormNum();

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        if (!(geometry is TriPyramid))
            return false;
        TriPyramid triPyramid = (TriPyramid)geometry;

        FormElement formElement = (FormElement)formInput.inputs[0];
        if (!IsTopEdge(triPyramid, formElement))
            return false;

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

        FormElement formElement = (FormElement)formInput.inputs[0];
        string[] fields = formElement.fields;
        int id1 = geometry.SignVertex(fields[0]);
        int id2 = geometry.SignVertex(fields[1]);
        FormNum formNum = (FormNum)formInput.inputs[2];
        TopLengthCondition condition = new TopLengthCondition(id1, id2, formNum.num);

        return condition;
    }

}

public class TopLengthConditionState : ConditionState
{
    new TopLengthCondition condition;
    TriPyramid geometry;

    public TopLengthConditionState(Tool tool, Condition condition, Geometry geometry) : base(tool, condition)
    {
        if (condition is TopLengthCondition)
            this.condition = (TopLengthCondition)condition;

        if (geometry is TriPyramid)
            this.geometry = (TriPyramid)geometry;
    }

    public override int[] DependVertices()
    {
        int[] ids = new int[] {
            condition.edge.id1,
            condition.edge.id2,
        };
        return ids;
    }

    public override FormInput Title()
    {
        FormElement formElement = new FormElement(2);
        formElement.fields[0] = geometry.VertexSign(condition.edge.id1);
        formElement.fields[1] = geometry.VertexSign(condition.edge.id2);

        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = formElement;
        formInput.inputs[1] = new FormText("=");
        formInput.inputs[2] = new FormNum(condition.length);

        return formInput;
    }

}