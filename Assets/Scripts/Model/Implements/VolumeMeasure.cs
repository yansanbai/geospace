using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeMeasure : Measure
{
    public FaceRefer face;

    public VolumeMeasure(int[] ids) : base()
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

        GizmoVolume gizmoVolume = new GizmoVolume(face);
        gizmos = new Gizmo[] { gizmoVolume };
    }
}


public class VolumeMeasureTool : MeasureTool
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
        VolumeMeasure measure = new VolumeMeasure(ids);

        return measure;
    }
}

public class VolumeMeasureState : MeasureState
{
    new VolumeMeasure measure;
    Geometry geometry;

    public VolumeMeasureState(Tool tool, Measure measure, Geometry geometry) : base(tool, measure)
    {
        if (measure is VolumeMeasure)
            this.measure = (VolumeMeasure)measure;

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

        string volume = geometry.Volume(faceIds);
        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormText("体积");
        formInput.inputs[1] = new FormText("=");
        formInput.inputs[2] = new FormText(volume);

        return formInput;
    }
}