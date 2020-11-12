using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TopBRightCondition : TriPyramidTopCondition
{
    public CornerRefer corner;


    public TopBRightCondition(int id1, int id2, int id3) : base(3)
    {
        this.corner = new CornerRefer(id1, id2, id3);

        GizmoRight gizmoRight = new GizmoRight(corner);
        gizmos = new Gizmo[] { gizmoRight };
    }
}

public class TopBRightConditionTool : TriPyramidConditionTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormElement(2);
        formInput.inputs[1] = new FormText("⊥");
        formInput.inputs[2] = new FormElement(2);

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        if (!(geometry is TriPyramid))
            return false;
        TriPyramid triPyramid = (TriPyramid)geometry;

        FormElement formElement1 = (FormElement)formInput.inputs[0];
        FormElement formElement2 = (FormElement)formInput.inputs[2];

        FormElement formElement = EdgesToCorner(formElement1, formElement2);
        if (!IsTopBCorner(triPyramid, formElement))
            return false;

        return true;
    }

    public override Condition GenerateCondition(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        FormElement formElement1 = (FormElement)formInput.inputs[0];
        FormElement formElement2 = (FormElement)formInput.inputs[2];

        FormElement formElement = EdgesToCorner(formElement1, formElement2);

        string[] fields = formElement.fields;
        int id1 = geometry.SignVertex(fields[0]);
        int id2 = geometry.SignVertex(fields[1]);
        int id3 = geometry.SignVertex(fields[2]);
        TopBRightCondition condition = new TopBRightCondition(id1, id2, id3);

        return condition;
    }
}

public class TopBRightConditionState : ConditionState
{
    new TopBRightCondition condition;
    TriPyramid geometry;

    public TopBRightConditionState(Tool tool, Condition condition, Geometry geometry): base(tool, condition)
    {
        if (condition is TopBRightCondition)
            this.condition = (TopBRightCondition)condition;

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
        string sign1 = geometry.VertexSign(condition.corner.id1);
        string sign2 = geometry.VertexSign(condition.corner.id2);
        string sign3 = geometry.VertexSign(condition.corner.id3);

        FormElement formElement1 = new FormElement(2);
        formElement1.fields[0] = sign2;
        formElement1.fields[1] = sign1;

        FormElement formElement2 = new FormElement(2);
        formElement2.fields[0] = sign2;
        formElement2.fields[1] = sign3;

        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = formElement1;
        formInput.inputs[1] = new FormText("⊥");
        formInput.inputs[2] = formElement2;

        return formInput;
    }

}

