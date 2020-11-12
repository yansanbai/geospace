using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceAreaMeasure : Measure
{
    public FaceRefer face;

    public SurfaceAreaMeasure(int[] ids) : base()
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

        GizmoSurface gizmoSurface = new GizmoSurface(face);
        gizmos = new Gizmo[] { gizmoSurface };
    }
}


public class SurfaceAreaMeasureTool : MeasureTool
{
    public override FormInput FormInput()
    {
        return null;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        return true;
    }

    public override Measure GenerateMeasure(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        if (!(geometry is ResolvedBody))
        {
            return null;
        }
        else
        {
            ResolvedBody resolvedBody = (ResolvedBody)geometry;
            if (!resolvedBody.isSpinned) {
                return null;
            }
            GeoCircular[] geoCirculars = geometry.GeoCirculars();
            if (geoCirculars.Length != 0)
            {
                GeoCircular geoCircular = geoCirculars[0];
                Circular circular = geoCircular.Circular();
                if (!circular.IsNormalCircular()) {
                    return null;
                }
            }
        }
        
        GeoVertex[] geoVertices = geometry.GeoVertices();
        int[] ids = new int[geoVertices.Length];
        int index = 0;
        foreach (GeoVertex vertice in geoVertices)
        {
            ids[index++] = vertice.Id;
        }
        SurfaceAreaMeasure measure = new SurfaceAreaMeasure(ids);

        return measure;
    }
}

public class SurfaceAreaMeasureState : MeasureState
{
    new SurfaceAreaMeasure measure;
    Geometry geometry;

    public SurfaceAreaMeasureState(Tool tool, Measure measure, Geometry geometry) : base(tool, measure)
    {
        if (measure is SurfaceAreaMeasure)
            this.measure = (SurfaceAreaMeasure)measure;

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

        string area = geometry.SurfaceArea(faceIds);
        // FormNum formNum = new FormNum(area);
        // formNum.format = UIConstants.AreaFormat;

        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormText("面积");
        formInput.inputs[1] = new FormText("=");
        formInput.inputs[2] = new FormText(area);

        return formInput;
    }
}