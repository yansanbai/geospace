using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornerAngleMeasure : Measure
{
    public CornerRefer corner;

    public CornerAngleMeasure(int id1, int id2, int id3) : base()
    {
        corner = new CornerRefer(id1, id2, id3);
    }

    public override void InitWithGeometry(Geometry geometry)
    {
        VertexUnit v1 = geometry.VertexUnit(corner.id1);
        VertexUnit v2 = geometry.VertexUnit(corner.id2);
        VertexUnit v3 = geometry.VertexUnit(corner.id3);

        dependencies = new List<VertexUnit>();
        dependencies.Add(v1);
        dependencies.Add(v2);
        dependencies.Add(v3);

        GizmoCorner gizmoCorner = new GizmoCorner(corner);
        GizmoAngle gizmoAngle = new GizmoAngle(corner);
        gizmos = new Gizmo[] { gizmoCorner, gizmoAngle };
    }
}
public class CornerAngleMeasureTool : MeasureTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormText("∠");
        formInput.inputs[1] = new FormElement(3);
        formInput.inputs[2] = new FormText("的角度");

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        FormElement formElement = (FormElement)formInput.inputs[1];
        if (!IsCorner(geometry, formElement))
            return false;
        return true;
    }

    public override Measure GenerateMeasure(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        FormElement formElement = (FormElement)formInput.inputs[1];
        string[] fields = formElement.fields;
        int i1 = geometry.SignVertex(fields[0]);
        int i2 = geometry.SignVertex(fields[1]);
        int i3 = geometry.SignVertex(fields[2]);
        CornerAngleMeasure measure = new CornerAngleMeasure(i1, i2, i3);

        return measure;
    }
}


public class CornerAngleMeasureState : MeasureState
{
    new CornerAngleMeasure measure;
    Geometry geometry;

    public CornerAngleMeasureState(Tool tool, Measure measure, Geometry geometry) : base(tool, measure)
    {
        if (measure is CornerAngleMeasure)
            this.measure = (CornerAngleMeasure)measure;

        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        int[] ids = new int[] {
            measure.corner.id1,
            measure.corner.id2,
            measure.corner.id3,
        };
        return ids;
    }

    public override FormInput Title()
    {
        CornerRefer corner = measure.corner;

        FormElement formElement = new FormElement(3);
        formElement.fields[0] = geometry.VertexSign(corner.id1);
        formElement.fields[1] = geometry.VertexSign(corner.id2);
        formElement.fields[2] = geometry.VertexSign(corner.id3);

        float angle = geometry.CornerAngle(corner.id1, corner.id2, corner.id3);
        FormNum formNum = new FormNum(angle);
        formNum.format = UIConstants.AngleFormat;

        FormInput formInput = new FormInput(4);

        formInput.inputs[0] = new FormText("∠");
        formInput.inputs[1] = formElement;
        formInput.inputs[2] = new FormText("=");
        formInput.inputs[3] = formNum;

        return formInput;
    }
}