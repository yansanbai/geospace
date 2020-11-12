using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Measure
{
    static int count = 0;

    public int id;
    public Gizmo[] gizmos;
    public List<VertexUnit> dependencies;

     public Measure()
    {
        id = count;
        count++;
    }

    public abstract void InitWithGeometry(Geometry geometry);

}

public abstract class MeasureTool
{
    public abstract FormInput FormInput();

    public abstract bool ValidateInput(Geometry geometry, FormInput formInput);

    public abstract Measure GenerateMeasure(Geometry geometry, FormInput formInput);

    protected bool IsEdge(Geometry geometry, FormElement formElement)
    {
        if (!formElement.IsFull())
            return false;
        if (formElement.count != 2)
            return false;

        string[] fields = formElement.fields;

        int id1 = geometry.SignVertex(fields[0]);
        int id2 = geometry.SignVertex(fields[1]);
        bool result = geometry.IsEdge(id1, id2) && (fields[0] != fields[1]);

        return result;
    }

    protected bool IsCorner(Geometry geometry, FormElement formElement)
    {
        if (!formElement.IsFull())
            return false;
        if (formElement.count != 3)
            return false;

        string[] fields = formElement.fields;

        int id1 = geometry.SignVertex(fields[0]);
        int id2 = geometry.SignVertex(fields[1]);
        int id3 = geometry.SignVertex(fields[2]);

        bool result = geometry.IsEdge(id1, id2) && geometry.IsEdge(id3, id2);
        result = result && (fields[0] != fields[1]) && (fields[0] != fields[2]) && (fields[1] != fields[2]);

        return result;
    }

    protected bool IsFace(Geometry geometry, FormElement formElement)
    {
        if (!formElement.IsFull())
            return false;
        if (formElement.count < 3)
            return false;

        string[] fields = formElement.fields;
        int[] ids = new int[fields.Length];
        for (int i = 0; i < fields.Length; i++)
            ids[i] = geometry.SignVertex(fields[i]);
        bool result = geometry.IsFace(ids);

        return result;
    }
}

public abstract class MeasureState : State
{
    public Measure measure;

    public MeasureState(Tool tool) : base(tool)
    {

    }

    public MeasureState(Tool tool, Measure measure) : base(tool)
    {
        this.measure = measure;
    }

}