using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineLengthMeasure : Measure
{
    public EdgeRefer edge;

    public LineLengthMeasure(int id1, int id2) : base()
    {
        edge = new EdgeRefer(id1, id2);
    }

    public override void InitWithGeometry(Geometry geometry)
    {
        VertexUnit v1 = geometry.VertexUnit(edge.id1);
        VertexUnit v2 = geometry.VertexUnit(edge.id2);

        dependencies = new List<VertexUnit>();
        dependencies.Add(v1);
        dependencies.Add(v2);

        GizmoLength gizmoLength = new GizmoLength(edge);
        gizmos = new Gizmo[] { gizmoLength };
    }
}

public class LineLengthMeasureTool : MeasureTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormText("线段");
        formInput.inputs[1] = new FormElement(2);
        formInput.inputs[2] = new FormText("的长度");

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        FormElement formElement = (FormElement)formInput.inputs[1];
        if (!IsEdge(geometry, formElement))
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
        LineLengthMeasure measure = new LineLengthMeasure(i1, i2);

        return measure;
    }
}

public class LineLengthMeasureState : MeasureState
{
    new LineLengthMeasure measure;
    Geometry geometry;

    public LineLengthMeasureState(Tool tool, Measure measure, Geometry geometry) : base(tool, measure)
    {
        if (measure is LineLengthMeasure)
            this.measure = (LineLengthMeasure)measure;

        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        int[] ids = new int[] {
            measure.edge.id1,
            measure.edge.id2,
        };
        return ids;
    }

    public override FormInput Title()
    {
        EdgeRefer edge = measure.edge;

        FormElement formElement = new FormElement(2);
        formElement.fields[0] = geometry.VertexSign(edge.id1);
        formElement.fields[1] = geometry.VertexSign(edge.id2);

        float length = geometry.EdgeLength(edge.id1, edge.id2);
        FormNum formNum = new FormNum(length);
        formNum.format = UIConstants.LengthFormat;

        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = formElement;
        formInput.inputs[1] = new FormText("=");
        formInput.inputs[2] = formNum;

        return formInput;
    }
}