using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Auxiliary
{
    static int count = 0;

    public int id;
    public VertexUnit[] units;
    public GeoElement[] elements;
    public Gizmo[] gizmos;
    public List<VertexUnit> dependencies = new List<VertexUnit>();


    public Auxiliary()
    {
        id = count;
        count++;
    }

    public abstract void InitWithGeometry(Geometry geometry);

    public abstract void RemoveAuxiliary();
}


public abstract class AuxiliaryTool
{
    public abstract FormInput FormInput();

    public abstract bool ValidateInput(Geometry geometry, FormInput formInput);

    public abstract Auxiliary GenerateAuxiliary(Geometry geometry, FormInput formInput);

    protected bool IsNewSign(Geometry geometry, FormElement formElement)
    {
        if (formElement.count != 1)
            return false;
        if (formElement.IsEmpty())
            return true;

        string[] fields = formElement.fields;

        int id = geometry.SignVertex(fields[0]);
        return id == -1;
    }

    protected string Sign(FormElement formElement)
    {
        if (formElement.count != 1)
            return null;
        string[] fields = formElement.fields;
        string sign = fields[0];
        if (sign == null || sign == "")
            return null;
        return sign;
    }

    protected bool IsVertex(Geometry geometry, FormElement formElement)
    {
        if (!formElement.IsFull())
            return false;
        if (formElement.count != 1)
            return false;

        string[] fields = formElement.fields;

        int id = geometry.SignVertex(fields[0]);
        bool result = id >= 0;

        return result;
    }

    protected bool IsTwoVertex(Geometry geometry, FormElement formElement)
    {
        if (!formElement.IsFull())
            return false;
        if (formElement.count != 2)
            return false;

        string[] fields = formElement.fields;

        int id1 = geometry.SignVertex(fields[0]);
        int id2 = geometry.SignVertex(fields[1]);
        bool result = (id1 >= 0 && id2 >= 0) && (fields[0] != fields[1]);

        return result;
    }

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

    protected bool IsPointCoordinate(Geometry geometry, FormNum formNum)
    {
        if (formNum.isEmpty)
            return false;
        return true;
    }
}

public abstract class AuxiliaryState : State
{
    public Auxiliary auxiliary;

    public AuxiliaryState(Tool tool) : base(tool)
    {

    }

    public AuxiliaryState(Tool tool, Auxiliary auxiliary) : base(tool)
    {
        this.auxiliary = auxiliary;
    }

}