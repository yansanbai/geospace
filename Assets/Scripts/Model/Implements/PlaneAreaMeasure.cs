using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAreaMeasure : Measure
{
    public FaceRefer face;

    public PlaneAreaMeasure(int[] ids) : base()
    {
        face = new FaceRefer(ids);
    }

    public override void InitWithGeometry(Geometry geometry)
    {
        int count = face.ids.Length;
        VertexUnit[] vertexUnits = new VertexUnit[count];
        for (int i = 0; i < count; i++)
            vertexUnits[i] = geometry.VertexUnit(face.ids[i]);

        dependencies = new List<VertexUnit>(vertexUnits);


        GizmoArea gizmoArea = new GizmoArea(face);
        gizmos = new Gizmo[] { gizmoArea };
    }
}


public class PlaneAreaMeasureTool : MeasureTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormText("平面");
        formInput.inputs[1] = new FormElement(-3);
        formInput.inputs[2] = new FormText("的面积");

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        FormElement formElement = (FormElement)formInput.inputs[1];
        if (!IsFace(geometry, formElement))
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
        int[] ids = new int[fields.Length];
        for (int i = 0; i < fields.Length; i++)
            ids[i] = geometry.SignVertex(fields[i]);
        PlaneAreaMeasure measure = new PlaneAreaMeasure(ids);

        return measure;
    }
}

public class PlaneAreaMeasureState : MeasureState
{
    new PlaneAreaMeasure measure;
    Geometry geometry;

    public PlaneAreaMeasureState(Tool tool, Measure measure, Geometry geometry) : base(tool, measure)
    {
        if (measure is PlaneAreaMeasure)
            this.measure = (PlaneAreaMeasure)measure;

        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        return measure.face.ids;
    }

    public override FormInput Title()
    {
        int[] faceIds = measure.face.ids;
        int len = faceIds.Length;

        FormElement formElement = new FormElement(len);
        for (int i = 0; i < len; i++)
        {
            formElement.fields[i] = geometry.VertexSign(faceIds[i]);
        }

        float area = geometry.FaceArea(faceIds);
        FormNum formNum = new FormNum(area);
        formNum.format = UIConstants.AreaFormat;

        FormInput formInput = new FormInput(4);

        formInput.inputs[0] = formElement;
        formInput.inputs[1] = new FormText("面积");
        formInput.inputs[2] = new FormText("=");
        formInput.inputs[3] = formNum;

        return formInput;
    }
}